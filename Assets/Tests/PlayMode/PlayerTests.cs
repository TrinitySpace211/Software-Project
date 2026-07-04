using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Komplette PlayerTests-Klasse in der CI deaktiviert (skipped).
// Diese Integrationstests instanziieren das echte Player-Prefab und lassen Player.Update()
// laufen. Player.Update() haengt von Laufzeit-/UI-Kontext ab, der im Test nicht gestellt wird:
//   - Player.cs:96 nutzt EventSystem.current (ist im Test null -> NullReference),
//   - das Prefab-EventSystem nutzt das alte StandaloneInputModule, das unter
//     "Input System (New)" pro Frame eine InvalidOperationException wirft.
// Test-seitig nicht sauber loesbar (Player BRAUCHT das EventSystem, das EventSystem WIRFT).
// Die uebrigen PlayMode-Tests (GasTank, DayNight, HealthBar, Menu, Options, ButtonHover)
// laufen sauber durch.
// TODO (Anton/Team): PlayerTests CI-tauglich machen - Player.Update von der direkten
// EventSystem.current-Abhaengigkeit entkoppeln (Null-Check) und/oder Active Input Handling
// projektweit auf "Both" stellen bzw. Prefab auf InputSystemUIInputModule migrieren.
//[Ignore("Headless nicht lauffaehig: Player.Update braucht Laufzeit-/UI-Kontext (EventSystem.current null, StandaloneInputModule vs. neues Input System). Siehe Klassen-Kommentar.")]
[TestFixture]
public class PlayerTests {

    private Player _playerPrefab;
    private PlayerInputHandler _playerInputHandler;
    private PlayerHealth _playerHealth;
    private PlayerWeaponSelector _weaponSelector;

    private Player _player;
    private PlayerAnimation _playerAnimation;
    private Camera _camera;

    private GunSO _assaultRifleSO;

    //Dynamic Objects
    private GameObject _groundPlane;
    private GameObject _playerInputHandlerObject;
    private GameObject _cameraObject;
    private GameObject _eventSystemObject;
    private GameObject _playerInstance;

    private EventSystem currentEventSystem;

    [OneTimeSetUp]
    public void PlayerIntegrationTest() {
        // Pfad korrigiert: das Prefab liegt im Unterordner ".../PrefabsAnton/Player/Player.prefab".
        // Vorher zeigte der Pfad auf ".../PrefabsAnton/Player.prefab" (ohne Unterordner) ->
        // LoadAssetAtPath lieferte null -> Instantiate(null) liess das gesamte Setup und damit
        // alle PlayerTests fehlschlagen. Tipp: feste Asset-Pfade brechen bei Ordner-Umzuegen;
        // robuster waere eine SerializeField-/Resources-Referenz.
        _playerPrefab = AssetDatabase.LoadAssetAtPath<Player>("Assets/Prefabs/PrefabsAnton/Player/Player.prefab");
        _assaultRifleSO = AssetDatabase.LoadAssetAtPath<GunSO>("Assets/Resources/ScriptableObjects/GunSOs/Assault_Rifle.asset");

        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60;
    }

    [SetUp]
    public void Setup() {
        // Erstelle Plane für den Raycast
        _groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _groundPlane.name = "Ground";
        _groundPlane.layer = 6; // Ground Layer
        _groundPlane.transform.position = new Vector3(0, 0, 0);
        _groundPlane.transform.localScale = new Vector3(15, 1, 15);

        // Erstelle einen GameObject für den PlayerInputHandler
        _playerInputHandlerObject = new GameObject("PlayerInputHandler");
        _playerInputHandler = _playerInputHandlerObject.AddComponent<PlayerInputHandler>();

        // Erstelle einen GameObject für die Camera
        _cameraObject = new GameObject("Camera");
        _camera = _cameraObject.AddComponent<Camera>();
        _camera.transform.position = new Vector3(0, 5.5f, -5f);
        _camera.transform.rotation = Quaternion.Euler(new Vector3(40f, 0f, 0f));

        // Player instanzieren + PlayerInputHandler und Camera initiieren
        _playerInstance = Object.Instantiate(_playerPrefab.gameObject);
        _player = _playerInstance.GetComponent<Player>();

        // PlayerAnimation Komponente holen und PlayerInputHandler initiieren
        _playerAnimation = _player.GetComponent<PlayerAnimation>();
        _playerAnimation.Construct(_playerInputHandler);

        // Erstelle einen GameObject für den PlayerHealth
        _playerHealth = _player.GetComponent<PlayerHealth>();
        _weaponSelector = _player.GetComponent<PlayerWeaponSelector>();

        _eventSystemObject = new GameObject("EventSystem");
        _eventSystemObject.AddComponent<EventSystem>();
        _eventSystemObject.AddComponent<InputSystemUIInputModule>();
        currentEventSystem = _eventSystemObject.GetComponent<EventSystem>();

        _player.Construct(_playerInputHandler, _playerHealth, _camera, _weaponSelector, currentEventSystem);
    }

    [TearDown]
    public void Teardown() {
        // WICHTIG: Lösche ALLES, was im Setup mit "new" oder "Instantiate" gebaut wurde
        if (_playerInstance != null) Object.DestroyImmediate(_playerInstance);
        if (_groundPlane != null) Object.DestroyImmediate(_groundPlane);
        if (_playerInputHandlerObject != null) Object.DestroyImmediate(_playerInputHandlerObject);
        if (_cameraObject != null) Object.DestroyImmediate(_cameraObject);
        if (_eventSystemObject != null) Object.DestroyImmediate(_eventSystemObject);

        // Referenzen nullen, um zu verhindern, dass der nächste Test auf Alte zugreift
        _player = null;
        _playerInputHandler = null;
        _camera = null;
        _playerAnimation = null;
        _playerHealth = null;
        _weaponSelector = null;
        currentEventSystem = null;
    }

    [UnityTest]
    public IEnumerator Player_MovesForwardAfterMovementInput() {
        // Speichere ursprüngliche Position
        float initialPositionZ = _player.transform.position.z;

        // Simuliere Bewegungsinput
        _playerInputHandler.SetMovementInput(Vector2.up);

        yield return null;

        Vector3 currentPlayerMovement = _player.GetCurrentPlayerMovement();

        //Debug.Log("Initial Pos: " + initialPositionY);
        //Debug.Log("After Input: " + _player.transform.position.y);

        // Prüfe, ob sich die Position geändert hat
        Assert.AreNotEqual(initialPositionZ, _player.transform.position.z, "Player sollte sich auf der 2D Y-Achse bewegen");
        Assert.Positive(currentPlayerMovement.normalized.z, "Player bewegt sich nach vorne");
    }

    [UnityTest]
    public IEnumerator Player_MovesBackwardAfterMovementInput() {
        // Simuliere Bewegungsinput
        _playerInputHandler.SetMovementInput(Vector2.down);

        yield return null;

        Vector3 currentPlayerMovement = _player.GetCurrentPlayerMovement();

        //Debug.Log("Current Player Movement: " + currentPlayerMovement);
        //Debug.Log("Current Player Movement Normalized: " + currentPlayerMovement.normalized);

        // Prüfe, ob sich die Position geändert hat
        Assert.AreNotEqual(Vector3.zero, currentPlayerMovement, "Player sollte sich auf der 2D Y-Achse bewegen");
        Assert.Negative(currentPlayerMovement.normalized.z, "Player bewegt sich nach hinten");
    }

    [UnityTest]
    public IEnumerator Player_MovesLeftAfterMovementInput() {
        // Speichere ursprüngliche Position
        float initialPositionX = _player.transform.position.x;

        // Simuliere Bewegungsinput
        _playerInputHandler.SetMovementInput(Vector2.left);

        yield return null;

        Vector3 currentPlayerMovement = _player.GetCurrentPlayerMovement();

        //Debug.Log("Initial Pos: " + initialPositionX);
        //Debug.Log("After Input: " + _player.transform.position.x);

        // Prüfe, ob sich die Position geändert hat
        Assert.AreNotEqual(initialPositionX, _player.transform.position.x, "Player sollte sich nicht auf der 2D X-Achse bewegen");
        Assert.Negative(currentPlayerMovement.normalized.x, "Player bewegt sich nach links");
    }

    [UnityTest]
    public IEnumerator Player_MovesRightAfterMovementInput() {
        // Speichere ursprüngliche Position
        float initialPositionX = _player.transform.position.x;

        // Simuliere Bewegungsinput
        _playerInputHandler.SetMovementInput(Vector2.right);

        yield return null;

        Vector3 currentPlayerMovement = _player.GetCurrentPlayerMovement();

        //Debug.Log("Initial Pos: " + initialPositionX);
        //Debug.Log("After Input: " + _player.transform.position.x);

        // Prüfe, ob sich die Position geändert hat
        Assert.AreNotEqual(initialPositionX, _player.transform.position.x, "Player sollte sich nicht auf der 2D X-Achse bewegen");
        Assert.Positive(currentPlayerMovement.normalized.x, "Player bewegt sich nach rechts");
    }

    [UnityTest]
    public IEnumerator Player_RotatesToMouseDirection() {
        // Speichere ursprüngliche Rotation
        _player.transform.rotation = Quaternion.Euler(Vector3.zero);
        Quaternion initialRotation = _player.transform.rotation;
        //Debug.Log("Initial Rotation: " + initialRotation.eulerAngles);

        // Simuliere Mausposition (Screen-Koordinaten)
        Vector2 mousePositionRight = new Vector2(500f, 500f);
        _playerInputHandler.SetMousePosition(mousePositionRight);

        for (int i = 0; i < 15f; i++) {
            yield return null;
        }

        Quaternion rotatedRotation = _player.transform.rotation;
        //Debug.Log("After Mouse Move: " + rotatedRotation.eulerAngles);

        // Prüfe, ob sich die Rotation geändert hat
        float rotationDifference = Quaternion.Angle(initialRotation, rotatedRotation);
        Assert.AreNotEqual(rotationDifference, 1f, $"Player sollte sich zur Maus drehen. Rotation Diff: {rotationDifference}");
    }

    [UnityTest]
    public IEnumerator Player_RotatesToInputDirection_AfterSprintInput() {
        // Speichere ursprüngliche Rotation
        _player.transform.rotation = Quaternion.Euler(Vector3.zero);
        //Quaternion initialRotation = _player.transform.rotation;
        //Debug.Log("Initial Rotation: " + initialRotation);

        _playerInputHandler.SetMovementInput(Vector2.up);
        _playerInputHandler.SetSprintInput(true);
        _playerInputHandler.SetMovementInput(Vector2.right);
        //Debug.Log("initialRotation: " + initialRotation.eulerAngles);

        for (int i = 0; i < 15f; i++) {
            yield return null;
        }

        Quaternion rotatedRotation = _player.transform.rotation;
        //Debug.Log("After Mouse Move: " + rotatedRotation.eulerAngles);

        // Prüfe, ob sich die Rotation geändert hat, trotz Maus Position
        float expectedY = 90f;
        Assert.AreEqual(expectedY, rotatedRotation.eulerAngles.y, 10f, $"Player sollte sich nach rechts (90°) drehen, ist aber {rotatedRotation.eulerAngles.y}°");
    }

    [UnityTest]
    public IEnumerator Player_AimingAfterAimKeyPressed() {
        yield return null;

        _weaponSelector.SelectAssaultRifle(_assaultRifleSO);

        for (int i = 0; i < 100f; i++) {
            yield return null;
        }

        // Speichern der Gewichtung
        float initialWeight = _player.GetAimLayerWeight();

        _playerInputHandler.SetAimingInput(true);

        yield return null;

        //Debug.Log(initialWeight + "..." + _player.GetAimLayerWeight());

        Assert.Greater(_player.GetAimLayerWeight(), initialWeight, "Player sollte mit der Waffe zielen");
    }

    [UnityTest]
    public IEnumerator Player_IsSprintingOnInput() {
        _playerInputHandler.SetMovementInput(Vector2.up);
        _playerInputHandler.SetSprintInput(true);

        yield return null;
        yield return null;

        Assert.IsTrue(_player.IsSprinting());
    }

}


