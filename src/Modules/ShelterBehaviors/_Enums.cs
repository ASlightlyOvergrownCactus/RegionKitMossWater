namespace RegionKit.Modules.ShelterBehaviors;
///<inheritdoc/>
public static class _Enums
{
	// /// <summary>
	// /// Signleton POM object for customizing shelter behaviors. required in a room for others to work
	// /// </summary>
	// public static readonly PlacedObject.Type ShelterBhvrManager = new("ShelterBhvrManager", true);
	/// <summary>
	/// An additional door
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrPlacedDoor = new(nameof(ShelterBhvrPlacedDoor), true);
	/// <summary>
	/// A zone where player can sleep
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrTriggerZone = new(nameof(ShelterBhvrTriggerZone), true);
	/// <summary>
	/// A zone where player can't sleep
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrNoTriggerZone = new(nameof(ShelterBhvrNoTriggerZone), true);
	/// <summary>
	/// Shows HoldToTrigger tutorial
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrHTTTutorial = new(nameof(ShelterBhvrHTTTutorial), true);
	/// <summary>
	/// An additional spawn position for the shelter
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrSpawnPosition = new(nameof(ShelterBhvrSpawnPosition), true);
	/// <summary>
	/// An additional spawn position for the shelter
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrConsumableShelter = new(nameof(ShelterBhvrConsumableShelter), true);
	/// <summary>
	/// A shelter where holding is required to trigger
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrHoldToTrigger = new(nameof(ShelterBhvrHoldToTrigger), true);
	/// <summary>
	/// Removes the vanilla door graphics
	/// </summary>
	public static readonly PlacedObject.Type ShelterBhvrDoorless = new(nameof(ShelterBhvrDoorless), true);
}
