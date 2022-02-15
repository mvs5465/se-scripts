readonly List<IMyDoor> _doors = new List<IMyDoor>();
Dictionary<string, int> doorStatuses = new Dictionary<string, int>();

public Program()
{
    GridTerminalSystem.GetBlocksOfType<IMyDoor>(_doors, door => door.IsSameConstructAs(Me));
    foreach (var door in _doors)
    {
        doorStatuses[door.CustomName] = 0;
    }
    Echo($"Found {doorStatuses.Count} doors");
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateSource)
{
    foreach (var door in _doors)
    {
        if (door.Status == DoorStatus.Open)
        {
            if (doorStatuses[door.CustomName] > 5)
            {
                door.CloseDoor();
                doorStatuses[door.CustomName] = 0;
            }
            else {
                doorStatuses[door.CustomName] = doorStatuses[door.CustomName] + 1;
            }
        }
        else
        {
            doorStatuses[door.CustomName] = 0;
        }
    }
}