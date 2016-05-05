using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Enemies
{
  /// <summary>
  /// An object for a single attack effect. For example an AoE effect or a single melee blow. Contains effect type and list of its subeffects.
  /// </summary>
  public class Effect
  {
    /// <summary>
    /// Type of the effect (like projectile, melee and so on).
    /// </summary>
    public string Type;
    /// <summary>
    /// How many times this effects hit. -1 means variable or unknown.
    /// </summary>
    public int HitCount;
    /// <summary>
    /// How long the attack effect lasts.
    /// </summary>
    public double HitLength;
    /// <summary>
    /// How often the effect hits. Used for auras since they don't have a proper length.
    /// </summary>
    public double HitFrequency;
    /// <summary>
    /// List of subeffects (like burning, damage and so on).
    /// </summary>
    public List<string> SubEffects = new List<string>();

    public Effect(string type)
    {
      HitCount = 1;
      HitLength = 0.0;
      HitFrequency = 0.0;
      Type = type;
    }

    /// <summary>
    /// Replaces enemy and other link tags with html.
    /// </summary>
    public void CreateLinks(List<string> paths, List<Enemy> enemies)
    {
      Type = LinkGenerator.CreateLinks(Type, paths, enemies);
      for (var i = 0; i < SubEffects.Count; i++)
        SubEffects[i] = LinkGenerator.CreateLinks(SubEffects[i], paths, enemies);
    }

    /// <summary>
    /// Generates the HTML representation.
    /// </summary>
    public string ToHtml(Attack owner, Enemy attackOwner, int baseIndent)
    {
      var htmlBuilder = new StringBuilder();
      Type = EffectHandler.HandleEffect(Type, this, owner, attackOwner);
      // Replace end dot with a double dot if the effect has sub effects (visually looks better).
      if (Type[Type.Length - 1] == '.' && SubEffects.Count > 0)
        Type = Type.Substring(0, Type.Length - 1) + ':';
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<p>").Append(Type).Append("</p>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<ul>").Append(Constants.LineEnding);
      foreach (var subEffect in SubEffects)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<li>");
        var str = EffectHandler.HandleEffect(subEffect, this, owner, attackOwner);
        htmlBuilder.Append(str.Replace("\\:", ":"));
        htmlBuilder.Append("</li>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("</ul>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }
  }
}
