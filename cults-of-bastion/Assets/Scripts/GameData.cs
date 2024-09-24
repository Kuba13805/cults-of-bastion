using System.Collections.Generic;
using Characters;
using Locations;
using Organizations;

/// <summary>
/// This class holds all data for the game. Location data, characters data, etc.
/// </summary>
public class GameData
{
    public Character PlayerCharacter;
    public Organization PlayerOrganization;
    
    public List<LocationData> LocationData = new();
    
    public List<Character> Characters = new();
    public List<CharacterConstructor> CharacterConstructors = new();
    
    public List<Organization> Organizations = new();
    public List<OrganizationConstructor> OrganizationConstructors = new();
}