using System.Collections.Generic;
using Characters;
using Locations;
/// <summary>
/// This class holds all data for the game. Location data, loaded characters, etc.
/// </summary>
public class GameData
{
    public List<LocationData> Locations = new();
    public List<LocationType> LocationTypes = new();
    public List<Character> Characters = new();
    public List<CharacterConstructor> CharacterConstructors = new();
        
    public readonly Dictionary<string, LocationType> LocationTypeDict = new();
}