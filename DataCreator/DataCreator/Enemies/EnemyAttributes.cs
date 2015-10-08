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
      Size = 1.0;
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
      Helper.ShowWarning("Rank " + Rank + " not recognized.");
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
      htmlBuilder.Append(" data-power=\"").Append(Power).Append("\"");
      htmlBuilder.Append(" data-precision=\"").Append(Precision).Append("\"");
      htmlBuilder.Append(" data-toughness=\"").Append(Toughness).Append("\"");
      htmlBuilder.Append(" data-vitality=\"").Append(Vitality).Append("\"");
      htmlBuilder.Append(" data-ferocity=\"").Append(Ferocity).Append("\"");
      htmlBuilder.Append(" data-condition=\"").Append(ConditionDamage).Append("\"");
      htmlBuilder.Append(" data-healing=\"").Append(HealingPower).Append("\"");
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

    public string GetDisplay()
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
      return display.Replace(" ", Constants.Space);
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
    public int Type { get; set; }

    [JsonProperty("rarity")]
    public int Rarity { get; set; }

    [JsonProperty("internalLevel")]
    public int InternalLevel { get; set; }

    [JsonProperty("skill_palette")]
    public List<SkillPalette> Skills { get; set; }

    public string ToHtml(string prefix)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-scale =\"").Append(Scale).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-type=\"").Append(Type).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-rarity=\"").Append(Rarity).Append("\"");
      htmlBuilder.Append(" data-weapon-").Append(prefix).Append("-level=\"").Append(InternalLevel).Append("\"");
      return htmlBuilder.ToString();
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