using DataCreator.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace DataCreator
{
  /// <summary>
  /// An object for datamined enemy information.
  /// </summary>
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

    private string rank;
    [JsonProperty("rank")]
    public string Rank
    {
      get
      {
        return rank;
      }

     set
      {
        if (value.StartsWith("rank_"))
          rank = value.Substring(5);
        else
          rank = value;
        if (rank.Equals("none"))
          rank = "normal";
        if (rank.Length == 0)
          ErrorHandler.ShowWarning("Missing info. Use \"rank='rank'\"!");
        if (!Constants.AvailableRanks.Contains(rank))
          ErrorHandler.ShowWarning("Rank " + rank + " not recognized.");

      }
    }

    [JsonProperty("flag")]
    public int Flag { get; set; }

    [JsonProperty("multipliers")]
    public Multipliers Multipliers { get; set; }

    [JsonProperty("sex")]
    public string Gender { get; set; }

    /// <summary>
    /// Relative size of the enemy. Unsure if this is based on some universal base size or model base size.
    /// </summary>
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

  /// <summary>
  /// An object for enemy attribute scaling.
  /// </summary>
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

  /// <summary>
  /// An object for enemy position in-game. This data has been collected manually with an automatic tool.
  /// </summary>
  public class Occurrence
  {
    /// <summary>
    /// Instance/map name where the enemy was found.
    /// </summary>
    [JsonProperty("location")]
    public string Location { get; set; }
    /// <summary>
    /// Level of the enemy in found location.
    /// </summary>
    [JsonProperty("level")]
    public int Level { get; set; }
  }

  /// <summary>
  /// An object for enemy race.
  /// </summary>
  public class Family
  {
    public Family()
    {
      Name = "";
      Guid = "";
    }

    /// <summary>
    /// Name for the race. This is often badly formatted or completely missing.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    public void SetName(string name)
    {
      if (name.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"race='value'.");
      Name = name;
    }
    /// <summary>
    /// Id for the race. This is used to generate proper race names.
    /// </summary>
    [JsonProperty("guid")]
    public string Guid { get; set; }

    /// <summary>
    /// Creates a properly formatted name.
    /// </summary>
    private string GetProperName()
    {
      string display = Helper.ToUpperAll(Name.Replace('_', ' '));
      // No idea if Ghost slaying also works against these. Separated just in case.
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
        // These have been figured out manually.
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

    /// <summary>
    /// Gets race with HTML ID formatting.
    /// </summary>
    public string GetInternal()
    {
      return GetProperName().ToLower().Replace(' ', '_');
    }

    /// <summary>
    /// Gets race with display formatting.
    /// </summary>
    /// <returns></returns>
    public string GetDisplay()
    {
      return GetProperName().Replace(" ", Constants.Space);
    }
  }

  /// <summary>
  /// An object for passive effects. No practical use so far (needs research).
  /// </summary>
  public class Passive
  {

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }
  }

  /// <summary>
  /// An object for enemy's weapons.
  /// </summary>
  public class Weapons
  {

    [JsonProperty("main_hand")]
    public Weapon Main { get; set; }

    [JsonProperty("off_hand")]
    public Weapon Offhand { get; set; }

    [JsonProperty("underwater")]
    public Weapon Underwater { get; set; }

    /// <summary>
    /// Returns HTML for enemy's weapons.
    /// </summary>
    public StringBuilder ToHtml()
    {
      var htmlBuilder = new StringBuilder();
      if (Main != null)
        htmlBuilder.Append(Main.ToHtml("main"));
      if (Offhand != null)
        htmlBuilder.Append(Offhand.ToHtml("off"));
      if (Underwater != null)
        htmlBuilder.Append(Underwater.ToHtml("water"));
      return htmlBuilder;
    }
  }

  /// <summary>
  /// An object for one weapon.
  /// </summary>
  public class Weapon
  {
    /// <summary>
    /// Id for this specific weapon and skills. Not in use.
    /// </summary>
    [JsonProperty("id")]
    public int id { get; set; }

    /// <summary>
    /// Enemy level's effect on weapon strength.
    /// </summary>
    [JsonProperty("scale")]
    public int Scale { get; set; }

    /// <summary>
    /// Weapon type (axe, dagger, rifle, etc.). Affects weapon strength range.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Rarity affects weapon strength.
    /// </summary>
    [JsonProperty("rarity")]
    public string Rarity { get; set; }

    /// <summary>
    /// This also affects weapon strength.
    /// </summary>
    [JsonProperty("internalLevel")]
    public int InternalLevel { get; set; }

    /// <summary>
    /// Skills available for this weapon id.
    /// </summary>
    [JsonProperty("skill_palette")]
    public List<SkillPalette> Skills { get; set; }

    /// <summary>
    /// Returns HTML For a weapon.
    /// </summary>
    public StringBuilder ToHtml(string weaponSlot)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(" data-weapon-").Append(weaponSlot).Append("-scale =\"").Append(Scale).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(weaponSlot).Append("-type=\"").Append(TypeToInt()).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(weaponSlot).Append("-rarity=\"").Append(RarityToInt()).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(weaponSlot).Append("-level=\"").Append(InternalLevel).Append("\"");
      return htmlBuilder;
    }

    /// <summary>
    /// Returns numerical value for rarity. Saves some space and is used like this on the weapon strength calculation.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Available weapon types.
    /// </summary>
    private List<string> WeaponTypes = new List<string>() {
      "sword", "hammer", "longbow", "shortbow", "axe", "dagger", "greatsword", "mace",
      "pistol", "polearm", "rifle", "scepter", "staff", "focus", "torch", "warhorn",
      "shield", "small_bundle", "large_bundle", "spear", "harpoon_gun", "trident", "toyweapon",
      "toyvisual", "maxitemweapontype"
    };

    /// <summary>
    /// Returns numerical value for weapon type. Saves some space and also used for weapon strength calculation.
    /// </summary>
    /// <returns></returns>
    private int TypeToInt()
    {
      var index = WeaponTypes.IndexOf(Type);
      if (index > -1)
        return index;
      ErrorHandler.ShowWarningMessage("Weapon type " + Type + " not recognized.");
      return 0;
    }

  }

  /// <summary>
  /// An object for one skill.
  /// </summary>
  public class SkillPalette
  {

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }

    /// <summary>
    /// Buff which is required to use this skill. Unsure if any real use.
    /// </summary>
    [JsonProperty("requiredBuff")]
    public Buff BuffRequirement { get; set; }

    /// <summary>
    /// Level requirement to use this skill. No idea if this has any effect.
    /// </summary>
    [JsonProperty("requiredLevel")]
    public int LevelRequirement { get; set; }

    /// <summary>
    /// Cooldown for this skill. This doesn't include casting time and other delays so not same as total recharge.
    /// </summary>
    [JsonProperty("cooldown")]
    public int Cooldown { get; set; }

    /// <summary>
    /// Casting time for this skill. Seems to NOT be reliable.
    /// </summary>
    [JsonProperty("casting_time")]
    public int CastTime { get; set; }

    /// <summary>
    /// Minimum range when AI attempts to use this skill.
    /// </summary>
    [JsonProperty("min_range")]
    public double MinimumRange { get; set; }

    /// <summary>
    /// Maximum range when AI attempts to use this skill.
    /// </summary>
    [JsonProperty("max_range")]
    public double MaxRange { get; set; }

    /// <summary>
    /// Any extra information on this skill.
    /// </summary>
    [JsonProperty("tags")]
    public Tags Tags { get; set; }
  }

  /// <summary>
  /// In-game pretty much everything is based on hidden buffs. This is currently pretty mysterious so no practical use for this.
  /// </summary>
  public class Buff
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }
  }

  /// <summary>
  /// Some special values for skills. No practical use so far (needs research).
  /// </summary>
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