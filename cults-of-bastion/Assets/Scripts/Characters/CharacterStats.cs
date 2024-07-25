namespace Characters
{
    public abstract class BaseStat
    {
        public int Value;
        public string Name;
        public string Desc;
    }

    public class CharacterStats
    {
        public Strength Strength = new();
        public Dexterity Dexterity = new();
        public Social Social = new();
        public Knowledge Knowledge = new();
        public Religious Religious = new();
        public Artistic Artistic = new();
        public Trickery Trickery = new();
        public Stewardship Stewardship = new();
    }
    public class Strength : BaseStat
    {
        public new static string Name => "stat_strength_name";
        public new static string Desc => "stat_strength_desc";
    }
    public class Dexterity : BaseStat
    {
        public new static string Name => "stat_dexterity_name";
        public new static string Desc => "stat_dexterity_desc";
    }
    public class Social : BaseStat
    {
        public new static string Name => "stat_social_name";
        public new static string Desc => "stat_social_desc";
    }
    public class Knowledge : BaseStat
    {
        public new static string Name => "stat_knowledge_name";
        public new static string Desc => "stat_knowledge_desc";
    }
    public class Religious : BaseStat
    {
        public new static string Name => "stat_religious_name";
        public new static string Desc => "stat_religious_desc";
    }
    public class Artistic : BaseStat
    {
        public new static string Name => "stat_artistic_name";
        public new static string Desc => "stat_artistic_desc";
    }
    public class Trickery : BaseStat
    {
        public new static string Name => "stat_trickery_name";
        public new static string Desc => "stat_trickery_desc";
    }
    public class Stewardship : BaseStat
    {
        public new static string Name => "stat_stewardship_name";
        public new static string Desc => "stat_stewardship_desc";
    }
}