namespace Characters
{
    public abstract class BaseStat
    {
        public int Value;
        public string Name;
        public string Desc;

        // Konstruktor bazowy, który może być wywoływany przez klasy dziedziczące.
        protected BaseStat(string name, string desc)
        {
            Name = name;
            Desc = desc;
        }
    }

    public class CharacterStats
    {
        public Strength Strength = new Strength();
        public Dexterity Dexterity = new Dexterity();
        public Social Social = new Social();
        public Knowledge Knowledge = new Knowledge();
        public Religious Religious = new Religious();
        public Artistic Artistic = new Artistic();
        public Trickery Trickery = new Trickery();
        public Stewardship Stewardship = new Stewardship();
    }

    public class Strength : BaseStat
    {
        public Strength() : base("stat_strength_name", "stat_strength_desc") { }
    }

    public class Dexterity : BaseStat
    {
        public Dexterity() : base("stat_dexterity_name", "stat_dexterity_desc") { }
    }

    public class Social : BaseStat
    {
        public Social() : base("stat_social_name", "stat_social_desc") { }
    }

    public class Knowledge : BaseStat
    {
        public Knowledge() : base("stat_knowledge_name", "stat_knowledge_desc") { }
    }

    public class Religious : BaseStat
    {
        public Religious() : base("stat_religious_name", "stat_religious_desc") { }
    }

    public class Artistic : BaseStat
    {
        public Artistic() : base("stat_artistic_name", "stat_artistic_desc") { }
    }

    public class Trickery : BaseStat
    {
        public Trickery() : base("stat_trickery_name", "stat_trickery_desc") { }
    }

    public class Stewardship : BaseStat
    {
        public Stewardship() : base("stat_stewardship_name", "stat_stewardship_desc") { }
    }
}