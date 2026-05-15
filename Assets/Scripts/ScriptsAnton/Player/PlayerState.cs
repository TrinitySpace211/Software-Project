using UnityEngine;

/// <summary>
/// An Enum with all the states of the Player
/// </summary>
public enum PlayerMovementState {
    Idling = 0,
    Walking = 1,
    Sprinting = 2,
    Jumping = 3,
    Falling = 4,
    Strafing = 5,
}

/// <summary>
/// A class to Update the Player Movement state
/// </summary>
public class PlayerState : MonoBehaviour {

    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;

    /// <summary>
    /// Updates the Movement state
    /// </summary>
    /// <param name="playerMovementState">what state it should update to</param>
    public void SetPlayerMovementState(PlayerMovementState playerMovementState) {
        CurrentPlayerMovementState = playerMovementState;
    }
}
