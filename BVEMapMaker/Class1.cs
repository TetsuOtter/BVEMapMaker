using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BVEMapMaker
{
  public class Map
  {
    //改行は自動挿入

    /// <summary>
    /// OutputするSyntaxのListのためのclass
    /// </summary>
    private class OutputList
    {
      public OutputList(double dst,string stx)
      {
        Distance = dst;
        Syntax.Add(stx);
      }

      /// <summary>
      /// 距離程
      /// </summary>
      public double Distance;
      /// <summary>
      /// SyntaxのList Addしてく。
      /// </summary>
      public List<string> Syntax;
    }
    static List<OutputList> Output = new List<OutputList>();
    StreamWriter writer = new StreamWriter("Map.txt", true, Encoding.UTF8);
    string BVEMapHeader;
    int VNum = new int();
    /// <summary>
    /// BVEMapMakerを初期化します。
    /// </summary>
    /// <param name="VersionNum">MapMakerの利用バージョン</param>
    public Map(int VersionNum)
    {
      VNum = VersionNum;
      if (VNum == 100)
      {
        BVEMapHeader = "BveTs Map 2.02\n";
      }
    }

    /// <summary>
    /// 自軌道の平面曲線に関するクラス
    /// </summary>
    public class Curve
    {
      /// <summary>
      /// カントを角度に換算するために、0m時点での自線の軌間を1.067[m]に設定する。
      /// </summary>
      public void SetGauge() => SetGauge(0, 1.067);
      /// <summary>
      /// カントを角度に換算するために、0m時点での自線の軌間を指定の値に設定する。
      /// </summary>
      /// <param name="Gauge">指定する軌間[m]</param>
      public void SetGauge(float Gauge) => SetGauge(0, Gauge);
      /// <summary>
      /// カントを角度に換算するために、指定の地点からの自線の軌間を指定の値に設定する。
      /// </summary>
      /// <param name="Distance">指定の地点の距離程[m]</param>
      /// <param name="Gauge">指定する軌間[m]</param>
      public void SetGauge(double Distance, double Gauge) => ListAdd(Distance, "Curve.SetGauge(" + Gauge.ToString() + ");");

      /// <summary>
      /// 距離程0m以降のカントの回転中心位置を変更する
      /// </summary>
      /// <param name="x">カントの回転中心位置[m](正:曲線の内側 / 負:曲線の外側)</param>
      public void SetCenter(double x) => SetCenter(0, x);
      /// <summary>
      /// 指定の地点以降のカントの回転中心位置を変更する
      /// </summary>
      /// <param name="D">変更開始地点[m]</param>
      /// <param name="x">カントの回転中心位置[m](正:曲線の内側 / 負:曲線の外側)</param>
      public void SetCenter(double D, double x) => ListAdd(D, "Curve.SetCenter(" + x.ToString() + ");");

      /// <summary>
      /// 距離程0m以降の緩和曲線の導出法を変更する
      /// </summary>
      /// <param name="f">導出法(0:サイン半波長逓減 / 1:直線逓減)</param>
      public void SetFunction(short f) => SetFunction(0, f);
      /// <summary>
      /// 指定の地点以降の緩和曲線の導出法を変更する
      /// </summary>
      /// <param name="D">変更開始する距離程</param>
      /// <param name="f">導出法(0:サイン半波長逓減 / 1:直線逓減)</param>
      public void SetFunction(double D, short f) => ListAdd(D, "Curve.SetFunction(" + f.ToString() + ");");

      /// <summary>
      /// 指定の地点から緩和曲線を開始する
      /// </summary>
      /// <param name="D">開始する距離程[m]</param>
      public void BeginTransition(double D) => ListAdd(D, "Curve.BeginTransition();");

      /// <summary>
      /// 指定の地点からカントの無い曲線を開始する
      /// </summary>
      /// <param name="D">開始する地点[m]</param>
      /// <param name="R">半径[m](正:右曲線 / 負:左曲線)</param>
      public void Begin(double D, double R) => ListAdd(D, "Curve.Begin(" + R.ToString() + ");");
      /// <summary>
      /// 指定の地点から曲線を開始する
      /// </summary>
      /// <param name="D">開始する地点[m]</param>
      /// <param name="R">半径[m](正:右曲線 / 負:左曲線)</param>
      /// <param name="C">カント[m](半径と正負を一致させる)</param>
      public void Begin(double D, double R, double C) => ListAdd(D, "Curve.Begin(" + R.ToString() + "," + C.ToString() + ");");

      /// <summary>
      /// 指定の地点でカーブを終了する
      /// </summary>
      /// <param name="D">終了する地点</param>
      public void End(double D) => ListAdd(D, "Curve.End();");

      /// <summary>
      /// ひとつ前の構文で設定された曲線の半径とカントの値を指定の地点まで継続させる
      /// </summary>
      /// <param name="D">距離程[m]</param>
      public void Interpolate(double D) => ListAdd(D, "Curve.Interpolate();");
      /// <summary>
      /// 指定の地点における曲線の半径を設定する(カントは前の構文の設定値が引き継がれる 構文間の数値は補完される)
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="R">半径[m](正:右曲線 / 負:左曲線)</param>
      public void Interpolate(double D, double R) => ListAdd(D, "Curve.Interpolate(" + R.ToString() + ");");
      /// <summary>
      /// 指定の地点における曲線の半径とカントを設定する(構文間の数値は補完される)
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="R">半径[m](正:右曲線 / 負:左曲線)</param>
      /// <param name="C">カント[m](半径と正負を一致させる)</param>
      public void Interpolate(double D, double R, double C) => ListAdd(D, "Curve.Interpolate(" + R.ToString() + "," + C.ToString() + ");");

      /// <summary>
      /// 指定の地点以降の曲線半径を設定する("Curve.Begin(Distance, Radius)"と同義)
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="R">半径[m](正:右曲線 / 負:左曲線)</param>
      public void Change(double D, double R) => ListAdd(D, "Curve.Change(" + R.ToString() + ");");
    }

    /// <summary>
    /// 自軌道の勾配に関するクラス
    /// </summary>
    public class Gradient
    {
      /// <summary>
      /// 指定の距離程から縦曲線を開始する
      /// </summary>
      /// <param name="D">開始する距離程[m]</param>
      public void BeginTransition(double D) => ListAdd(D, "Gradient.BeginTransition();");

      /// <summary>
      /// 指定の距離程で縦曲線を終了し、勾配を開始する
      /// </summary>
      /// <param name="D">開始する距離程[m]</param>
      /// <param name="G">勾配の大きさ[‰](正:上昇 / 負:下降)</param>
      public void Begin(double D, double G) => ListAdd(D, "Gradient.Begin(" + G.ToString() + ");");

      /// <summary>
      /// 指定の距離程で縦曲線を終了し、勾配を終了する(0‰にする)
      /// </summary>
      /// <param name="D">終了する距離程[m]</param>
      public void End(double D) => ListAdd(D, "Gradient.End();");

      /// <summary>
      /// 指定の距離程に勾配を設定する(Interpolateの勾配は線形補完される)
      /// </summary>
      /// <param name="D">設定する距離程[m]</param>
      /// <param name="G">勾配の大きさ[‰](正:上昇 / 負:下降)</param>
      public void Interpolate(double D, double G) => ListAdd(D, "Gradient.Interpolate(" + G.ToString() + ");");
      /// <summary>
      /// 指定の距離程まで、一つ前に設定された勾配を継続する。
      /// </summary>
      /// <param name="D">設定する距離程[m]</param>
      public void Interpolate(double D) => ListAdd(D, "Gradient.Interpolate();");
    }

    public class Track
    {
      public void XInterpolate(double D, string TN, double x, double r) => ListAdd(D, "");
    }

    /// <summary>
    /// OutputListに構文を追加する
    /// </summary>
    /// <param name="Distance">距離程</param>
    /// <param name="Syntax">構文</param>
    static private void ListAdd(double Distance, string Syntax)
    {
      try
      {
        int Ind = Output.FindIndex(OutputList => OutputList.Distance == Distance);
        Output[Ind].Syntax.Add(Syntax);
      }
      catch (ArgumentNullException) { Output.Add(new OutputList(Distance, Syntax)); }
    }
  }
}
