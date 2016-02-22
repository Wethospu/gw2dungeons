using DataCreator.Enemies;
using DataCreator.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace DataCreator
{
  public class EnemyAttributes
  {
    public EnemyAttributes()
    {
      Name = "";
      Occurrences = null;
      Description = "";
      Rank = "";
      Flag = 0;
      Multipliers = new Multipliers();
      Gender = "";
      Size = 0.0;
      Family = new Family();
      Passive = new Passive();
      Weapons = new Weapons();
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("occurrences")]
    public Occurrence[] Occurrences { get; set; }

    [JsonProperty("rank")]
    public string Rank { get; set; }

    public string GetRank()
    {
      if (Rank.Equals("rank_none"))
        return "normal";
      if (Rank.Equals("rank_veteran"))
        return "veteran";
      if (Rank.Equals("rank_elite"))
        return "elite";
      if (Rank.Equals("rank_champion"))
        return "champion";
      if (Rank.Equals("rank_legendary"))
        return "legendary";
      ErrorHandler.ShowWarning("Rank " + Rank + " not recognized.");
      return "normal";
    }

    [JsonProperty("flag")]
    public int Flag { get; set; }

    [JsonProperty("multipliers")]
    public Multipliers Multipliers { get; set; }

    [JsonProperty("sex")]
    public string Gender { get; set; }

    [JsonProperty("size")]
    public double Size { get; set; }

    [JsonProperty("family")]
    public Family Family { get; set; }

    [JsonProperty("passive")]
    public Passive Passive { get; set; }

    [JsonProperty("weapons")]
    public Weapons Weapons { get; set; }

    public string ToHtml()
    {
      var htmlBuilder = new StringBuilder();
      if (Multipliers != null)
        htmlBuilder.Append(Multipliers.ToHtml());
      if (Weapons != null)
        htmlBuilder.Append(Weapons.ToHtml());
      return htmlBuilder.ToString();
    }
  }

  public class Multipliers
  {
    public Multipliers()
    {
      Power = -1.0;
      Precision = -1.0;
      Toughness = -1.0;
      Vitality = -1.0;
      Ferocity = -1.0;
      HealingPower = -1.0;
      ConditionDamage = -1.0;
      HealthMultiplier = -1.0;
    }

    [JsonProperty("power")]
    public double Power { get; set; }

    [JsonProperty("precision")]
    public double Precision { get; set; }

    [JsonProperty("toughness")]
    public double Toughness { get; set; }

    [JsonProperty("vitality")]
    public double Vitality { get; set; }

    [JsonProperty("ferocity")]
    public double Ferocity { get; set; }

    [JsonProperty("healing_power")]
    public double HealingPower { get; set; }

    [JsonProperty("condition_damage")]
    public double ConditionDamage { get; set; }

    [JsonProperty("health_multiplier")]
    public double HealthMultiplier { get; set; }

    public string ToHtml()
    {
      var htmlBuilder = new StringBuilder();
      if (Power > -0.1)
        htmlBuilder.Append(" data-power=\"").Append(Power).Append("\"");
      if (Precision > -0.1)
        htmlBuilder.Append(" data-precision=\"").Append(Precision).Append("\"");
      if (Toughness > -0.1)
        htmlBuilder.Append(" data-toughness=\"").Append(Toughness).Append("\"");
      if (Vitality > -0.1)
        htmlBuilder.Append(" data-vitality=\"").Append(Vitality).Append("\"");
      if (Ferocity > -0.1)
        htmlBuilder.Append(" data-ferocity=\"").Append(Ferocity).Append("\"");
      if (ConditionDamage > -0.1)
        htmlBuilder.Append(" data-condition=\"").Append(ConditionDamage).Append("\"");
      if (HealingPower > -0.1)
        htmlBuilder.Append(" data-healing=\"").Append(HealingPower).Append("\"");
      if (HealthMultiplier > -0.1)
        htmlBuilder.Append(" data-health=\"").Append(HealthMultiplier).Append("\"");
      return htmlBuilder.ToString();
    }
  }

  public class Occurrence
  {

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }
  }

  public class Family
  {
    public Family()
    {
      Name = "";
      Guid = "";
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("guid")]
    public string Guid { get; set; }

    private string ProcessRace()
    {
      string display = Helper.ToUpperAll(Name.Replace('_', ' '));
      if (display.Equals("Ghost"))
        display = "Spectre";
      if (display.Equals("Ascalonian Ghost"))
        display = "Ghost";
      if (display.Equals("Undead Minions"))
        display = "Undead";
      if (display.Equals("Scarlet Minion"))
        display = "Aetherblade";
      if (display.Equals("Fleshreaver"))
        display = "Demon";
      if (display.Equals("Icebrood Minion"))
        display = "Icebrood";
      if (display.Equals("Flame Legion Charr"))
        display = "Flame Legion";
      if (display.StartsWith("Twisted Watchwork"))
        display = "Twisted Watchwork";
      if (display.Equals("") && !Guid.Equals(""))
      {
        if (Guid.Equals("e36b4acd-94fc-4710-94d7-b5b7176a5925"))
          display = "Ambient";
        else if (Guid.Equals("2dbe609b-56fd-4cae-9f9a-ba44430713b9"))
          display = "Robot";
        else if (Guid.Equals("59ba03e5-fb06-4f56-bfc4-846e1f07135b"))
          display = "Soldier";
        else if (Guid.Equals("0867b7c7-e5e6-4481-80ae-43c0ed9243be") || Guid.Equals("c49acb88-bd59-462d-acd1-bb196d998490"))
          display = "Other";
        else if (Guid.Equals("4294cd11-911b-47bc-b3b1-2cc6ad435f28"))
          display = "Molten";
        else
          ErrorHandler.ShowWarningMessage("Race with guid " + Guid + " not handled.");
      }
      return display;
    }

    public string GetInternal()
    {
      return ProcessRace().ToLower().Replace(' ', '_');
    }

    public string GetDisplay()
    {
      return ProcessRace().Replace(" ", Constants.Space);
    }
  }

  public class Passive
  {

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }
  }

  public class Weapons
  {

    [JsonProperty("main_hand")]
    public Weapon Main { get; set; }

    [JsonProperty("off_hand")]
    public Weapon Offhand { get; set; }

    [JsonProperty("underwater")]
    public Weapon Underwater { get; set; }

    public string ToHtml()
    {
      var htmlBuilder = new StringBuilder();
      if (Main != null)
        htmlBuilder.Append(Main.ToHtml("main"));
      if (Offhand != null)
        htmlBuilder.Append(Offhand.ToHtml("off"));
      if (Underwater != null)
        htmlBuilder.Append(Underwater.ToHtml("water"));
      return htmlBuilder.ToString();
    }
  }

  public class Weapon
  {

    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("scale")]
    public int Scale { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("rarity")]
    public string Rarity { get; set; }

    [JsonProperty("internalLevel")]
    public int InternalLevel { get; set; }

    [JsonProperty("skill_palette")]
    public List<SkillPalette> Skills { get; set; }

    public string ToHtml(string prefix)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-scale =\"").Append(Scale).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-type=\"").Append(TypeToInt()).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-rarity=\"").Append(RarityToInt()).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-level=\"").Append(InternalLevel).Append("\"");
      return htmlBuilder.ToString();
    }

    private int RarityToInt()
    {
      if (Rarity.Equals("basic"))
        return 1;
      if (Rarity.Equals("fine"))
        return 2;
      if (Rarity.Equals("masterwork"))
        return 3;
      if (Rarity.Equals("rare"))
        return 4;
      if (Rarity.Equals("exotic"))
        return 5;
      if (Rarity.Equals("ascended") || Rarity.Equals("legendary"))
        return 6;
      return 0;
    }

    private List<string> WeaponTypes = new List<string>() {
      "sword", "hammer", "longbow", "shortbow", "axe", "dagger", "greatsword", "mace",
      "pistol", "polearm", "rifle", "scepter", "staff", "focus", "torch", "warhorn",
      "shield", "small_bundle", "large_bundle", "spear", "harpoon_gun", "trident", "toyweapon",
      "toyvisual", "maxitemweapontype"
    };

    private int TypeToInt()
    {
      var index = WeaponTypes.IndexOf(Type);
      if (index > -1)
        return index;
      ErrorHandler.ShowWarningMessage("Weapon type " + Type + " not recognized.");
      return 0;
    }

  }

  public class SkillPalette
  {

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("requiredBuff")]
    public Buff BuffRequirement { get; set; }

    [JsonProperty("requiredLevel")]
    public int LevelRequirement { get; set; }

    [JsonProperty("cooldown")]
    public int Cooldown { get; set; }

    [JsonProperty("casting_time")]
    public int CastTime { get; set; }

    [JsonProperty("min_range")]
    public double MinimumRange { get; set; }

    [JsonProperty("max_range")]
    public double MaxRange { get; set; }

    [JsonProperty("tags")]
    public Tags Tags { get; set; }
  }

  public class Buff
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }
  }

  public class Tags
  {
    [JsonProperty("Damage Multiplier")]
    public double Coefficient { get; set; }

    [JsonProperty("Trait Damage Multiplier")]
    public double CoefficientTrait { get; set; }

    [JsonProperty("Splash Damage Multiplier")]
    public double CoefficientSplash { get; set; }
  }
}