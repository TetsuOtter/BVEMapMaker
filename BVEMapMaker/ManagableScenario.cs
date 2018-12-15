using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.BVEMapMaker
{
  /// <summary>専用ソフトにより自由にMapを組み合わせさせるためのインターフェース</summary>
  public interface IMngableScenario
  {
    /// <summary>路線情報を必要とされた際に呼び出される</summary>
    ScenarioInfo SetScenarioVersion();
    /// <summary>保安装置が設定された際に呼び出される</summary>
    void SetATS(ATSInfo ats);
  }

  /// <summary>路線情報をまとめている</summary>
  public class ScenarioInfo
  {
    /// <summary>路線名</summary>
    public string ScenarioName { get; }
    /// <summary>路線のバージョン</summary>
    public string ScenarioVersion { get; }
    /// <summary>製作者名</summary>
    public string ProducerName { get; }
    /// <summary>最初のリリース日</summary>
    public DateTime FirstRelease { get; }
    /// <summary>更新日</summary>
    public DateTime Update { get; }
    /// <summary>使用可能な保安装置の種類</summary>
    public List<ATSInfo> UseableATS { get; }
  }
  /// <summary>保安装置(プラグイン)の列挙子</summary>
  public enum ATSInfo
  {
    /// <summary>保安装置なし</summary>
    None,
    /// <summary>その他</summary>
    Other,
    /// <summary>Conv付属PI(停通防止警報繰り返し再生版)</summary>
    Default_ATS1,
    /// <summary>Conv付属PI(停通防止警報1回再生版)</summary>
    Defalut_ATS2,
    Metro_ATC_CS,
    Metro_ATC_WS,
    Metro_ATS_TOQ,
    Metro_ATS_TOB,
    Metro_ATS_Seib,
    Metro_ATS_IZQ,
    Metro_ATS_S,
    Metro_ATS_Sn,
    Metro_ATS_P,
    GAP_ATS_S,
    GAP_ATS_SWP2,
    GAP_ATS_Sn,
    GAP_ATS_SnP,
    GAP_ATS_SnPST,
    GAP_ATS_FQ,
    GAP_ATC_6,
    GAP_ATS_Ps,
    KO_ATC,
    PT_ATS_ST,
    PT_ATS_ST_PT_DP,
    PT_ATS_P_ST,
    PT_ATS_BaseP,
    PT_ATS_M_CSC_ST,
    PT_ATS_MBaseP_CSC_ST,
    PT_DenGo,
    PT_TASC,
    PT_FormDoor,
    PT_HalfSecClock,
    Delta_TASC,
    ReOrg_ATC_D,
    No_Gene_ATS_Sn,
    No_Gene_ATS_P,
    No_Gene_ATS_Ps,
    No_Gene_ATC,
    No_Gene_TASC,
    Ask_ATS_SnPs,
    Ask_ATS_P,
    Ask_ATS_10,
    Kiku_ATC_6,
    Kiku_TIMS,
    NNN2_ATS_C,
    NNN2_ATS_Route1,
    Oer_ATS_OM,
    Oer_ATS_DP,
    Oer_ATC_CS
  }

  /// <summary>路線情報を必要とされた際に呼び出される</summary>
  public class ManagableScenario
  {
  }
}
