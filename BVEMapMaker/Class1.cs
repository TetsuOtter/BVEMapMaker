using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRtec.BVEMapMaker
{
  /// <summary>
  /// BVE Mapファイル作成用クラス
  /// </summary>
  public class Map
  {
    //改行は自動挿入
    /// <summary>
    /// Mapクラスのインスタンスを初期化
    /// </summary>
    /// <param name="VersionNum">クラスバージョン情報</param>
    public Map(int VersionNum)
    {
      VNum = VersionNum;
      if (VNum == 100)
      {
        BVEMapHeader = "BveTs Map 2.02\n";
      }
    }

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
      public List<string> Syntax = new List<string>();
    }
    static List<OutputList> Output = new List<OutputList>();
    static string BVEMapHeader;
    int VNum = new int();
    /// <summary>
    /// Mapファイルを出力する
    /// </summary>
    /// <param name="MapName">マップファイル名</param>
    public void WriteMap(string MapName)
    {
      using(StreamWriter sw=new StreamWriter(MapName, false, Encoding.UTF8))
      {
        sw.WriteLine(BVEMapHeader);
        Console.WriteLine(BVEMapHeader);
        //Output.Sort((a, b) => (int)(a.Distance - b.Distance));
        //Console.WriteLine(Output.Count());
        //Console.WriteLine(Output[4]);
        //Console.ReadKey(false);
        for (int i = 0; i < Output.Count; i++)
        {
          try
          {
            sw.WriteLine(Output[i].Distance.ToString() + ";");
            Console.WriteLine(Output[i].Distance.ToString() + ";");
            for (int n = 0; n < Output[i].Syntax.Count(); n++)
            {
              sw.WriteLine(Output[i].Syntax[n]);
              Console.WriteLine(Output[i].Syntax[n]);
            }
          }
          catch(Exception e)
          {
            Console.WriteLine(e.Message);
            Console.WriteLine("[Enter]");
            Console.ReadKey(false);
          }
        }
        sw.Flush();
        Output.Clear();
        Console.WriteLine("MapWrite Comp. [Enter]");
        Console.ReadKey(false);
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
    /// <summary>
    /// 他軌道の位置などを設定するクラス
    /// </summary>
    public class Track
    {
      /// <summary>
      /// 指定の軌道について、一つ前に設定されたX軸方向位置と平面曲線相対半径を指定の位置で引き継いで設定する
      /// </summary>
      /// <param name="D">設定する位置[m]</param>
      /// <param name="TN">軌道名称</param>
      public void XInterpolate(double D, string TN) => ListAdd(D, "Track['" + TN + "'].X.Interpolate();");
      /// <summary>
      /// 指定の軌道について、指定の距離程におけるX軸方向位置を設定する(平面曲線相対半径は一つ前の値が引き継がれる)
      /// </summary>
      /// <param name="D">設定する位置[m]</param>
      /// <param name="TN">軌道名称</param>
      /// <param name="x">X軸方向位置[m]</param>
      public void XInterpolate(double D, string TN, double x) => ListAdd(D, "Track['" + TN + "'].X.Interpolate(" + x.ToString() + ");");
      /// <summary>
      /// 指定の軌道について、指定の距離程におけるX軸方向位置と平面曲線相対半径を設定する
      /// </summary>
      /// <param name="D">設定する位置[m]</param>
      /// <param name="TN">軌道名称</param>
      /// <param name="x">X軸方向位置[m]</param>
      /// <param name="r">平面曲線相対半径[m]</param>
      public void XInterpolate(double D, string TN, double x, double r) => ListAdd(D, "Track['" + TN + "'].X.Interpolate(" + x.ToString() + ", " + r.ToString() + ");");

      /// <summary>
      /// 指定の軌道について、一つ前に設定されたY軸方向位置と縦曲線相対半径を指定の位置で引き継いで設定する
      /// </summary>
      /// <param name="D">設定する位置[m]</param>
      /// <param name="TN">軌道名称</param>
      public void YInterpolate(double D, string TN) => ListAdd(D, "Track['" + TN + "'].Y.Interpolate();");
      /// <summary>
      /// 指定の軌道について、指定の距離程におけるY軸方向位置を設定する(縦曲線相対半径は一つ前の値が引き継がれる)
      /// </summary>
      /// <param name="D">設定する位置[m]</param>
      /// <param name="TN">軌道名称</param>
      /// <param name="Y">Y軸方向位置[m]</param>
      public void YInterpolate(double D, string TN, double Y) => ListAdd(D, "Track['" + TN + "'].Y.Interpolate(" + Y.ToString() + ");");
      /// <summary>
      /// 指定の軌道について、指定の距離程におけるY軸方向位置と縦曲線半径を設定する
      /// </summary>
      /// <param name="D">設定する位置[m]</param>
      /// <param name="TN">軌道名称</param>
      /// <param name="Y">Y軸方向位置[m]</param>
      /// <param name="r">縦曲線相対半径[m]</param>
      public void YInterpolate(double D, string TN, double Y, double r) => ListAdd(D, "Track['" + TN + "'].Y.Interpolate(" + Y.ToString() + ", " + r.ToString() + ");");


      /// <summary>
      /// 指定の軌道について、指定の距離程における位置や半径を自線と同一にする
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="TN">軌道名</param>
      public void Position(double D, string TN) => Position(D, TN, 0, 0, 0, 0);
      /// <summary>
      /// 指定の軌道について、指定の距離程におけるX軸方向位置を設定する(Y軸方向位置と各曲線相対半径は0[m])
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="TN">軌道名</param>
      /// <param name="X">X軸方向位置[m]</param>
      public void Position(double D, string TN, double X) => Position(D, TN, X, 0, 0, 0);
      /// <summary>
      /// 指定の軌道について、指定の距離程における位置を設定する(各曲線相対半径は0[m])
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="TN">軌道名</param>
      /// <param name="X">X軸方向位置[m]</param>
      /// <param name="Y">Y軸方向位置[m]</param>
      public void Position(double D, string TN, double X, double Y) => Position(D, TN, X, Y, 0, 0);
      /// <summary>
      /// 指定の軌道について、指定の距離程における位置と平面曲線相対半径を設定する(縦曲線相対半径は0[m])
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="TN">軌道名</param>
      /// <param name="X">X軸方向位置[m]</param>
      /// <param name="Y">Y軸方向位置[m]</param>
      /// <param name="Rx">平面曲線相対半径[m]</param>
      public void Position(double D, string TN, double X, double Y, double Rx) => Position(D, TN, X, Y, Rx, 0);
      /// <summary>
      /// 指定の軌道について、指定の距離程における位置と半径を設定する
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="TN">軌道名</param>
      /// <param name="X">X軸方向位置[m]</param>
      /// <param name="Y">Y軸方向位置[m]</param>
      /// <param name="Rx">平面曲線相対半径[m]</param>
      /// <param name="Ry">縦曲線相対半径[m]</param>
      public void Position(double D, string TN, double X, double Y, double Rx, double Ry) => ListAdd(D, "Track['" + TN + "'].Position(" + X.ToString() + ", " + Y.ToString() + ", " + Rx.ToString() + ", " + Ry.ToString() + ");");

      /// <summary>
      /// 他軌道のカントについて設定する
      /// </summary>
      public class Cant
      {
        /// <summary>
        /// カントを角度に換算するために、0m時点での他線の軌間を1.067[m]に設定する。
        /// </summary>
        /// <param name="TN">軌道名</param>
        public void SetGauge(string TN) => SetGauge(0, TN, 1.067);
        /// <summary>
        /// カントを角度に換算するために、0m時点での他線の軌間を指定の値に設定する。
        /// </summary>
        /// <param name="TN">軌道名</param>
        /// <param name="Gauge">指定する軌間[m]</param>
        public void SetGauge(string TN, double Gauge) => SetGauge(0, TN, Gauge);
        /// <summary>
        /// カントを角度に換算するために、指定の地点からの他線の軌間を指定の値に設定する。
        /// </summary>
        /// <param name="Distance">指定の地点の距離程[m]</param>
        /// <param name="TN">軌道名</param>
        /// <param name="Gauge">指定する軌間[m]</param>
        public void SetGauge(double Distance, string TN, double Gauge) => ListAdd(Distance, "Track['" + TN + "'].Cant.SetGauge(" + Gauge.ToString() + ");");

        /// <summary>
        /// 距離程0m以降のカントの回転中心位置を変更する
        /// </summary>
        /// <param name="TN">軌道名</param>
        /// <param name="x">カントの回転中心位置[m](正:曲線の内側 / 負:曲線の外側)</param>
        public void SetCenter(string TN, double x) => SetCenter(0, TN, x);
        /// <summary>
        /// 指定の地点以降のカントの回転中心位置を変更する
        /// </summary>
        /// <param name="D">変更開始地点[m]</param>
        /// <param name="TN">軌道名</param>
        /// <param name="x">カントの回転中心位置[m](正:曲線の内側 / 負:曲線の外側)</param>
        public void SetCenter(double D,string TN, double x) => ListAdd(D, "Track['"+TN+"'].Cant.SetCenter(" + x.ToString() + ");");

        /// <summary>
        /// 距離程0m以降のカント逓減関数を変更する
        /// </summary>
        /// <param name="TN">軌道名</param>
        /// <param name="f">導出法(0:サイン半波長逓減 / 1:直線逓減)</param>
        public void SetFunction(string TN, short f) => SetFunction(0, TN, f);
        /// <summary>
        /// 指定の地点以降のカント逓減関数を変更する
        /// </summary>
        /// <param name="D">変更開始する距離程</param>
        /// <param name="TN">軌道名</param>
        /// <param name="f">導出法(0:サイン半波長逓減 / 1:直線逓減)</param>
        public void SetFunction(double D, string TN, short f) => ListAdd(D, "Track['" + TN + "'].Cant.SetFunction(" + f.ToString() + ");");

        /// <summary>
        /// 指定の地点からカントの逓減を開始する
        /// </summary>
        /// <param name="D">開始する距離程[m]</param>
        /// <param name="TN">軌道名</param>
        public void BeginTransition(double D, string TN) => ListAdd(D, "Track['" + TN + "'].Cant.BeginTransition();");

        /// <summary>
        /// 指定の地点でカント逓減を終了し、カントを一定に保つ
        /// </summary>
        /// <param name="D">開始する地点[m]</param>
        /// <param name="TN">軌道名</param>
        /// <param name="C">カント[m](半径と正負を一致させる)</param>
        public void Begin(double D, string TN, double C) => ListAdd(D, "Track['" + TN + "'].Cant.Begin(" + C.ToString() + ");");

        /// <summary>
        /// 指定の地点でカントを終了する
        /// </summary>
        /// <param name="D">終了する地点</param>
        /// <param name="TN">軌道名</param>
        public void End(double D, string TN) => ListAdd(D, "Track['" + TN + "'].Cant.End();");

        /// <summary>
        /// ひとつ前の構文で設定されたカントの値を指定の地点まで継続させる
        /// </summary>
        /// <param name="D">距離程[m]</param>
        /// <param name="TN">軌道名</param>
        public void Interpolate(double D, string TN) => ListAdd(D, "Track['" + TN + "'].Cant.Interpolate();");
        /// <summary>
        /// 指定の地点におけるカントを設定する(構文間の数値は補完される)
        /// </summary>
        /// <param name="D">距離程[m]</param>
        /// <param name="TN">軌道名</param>
        /// <param name="C">カント[m](半径と正負を一致させる)</param>
        public void Interpolate(double D, string TN, double C) => ListAdd(D, "Track['" + TN + "'].Cant.Interpolate(" + C.ToString() + ");");
      }

    }
    /// <summary>
    /// ストラクチャの位置などを設定するクラス
    /// </summary>
    public class Structure
    {
      /// <summary>
      /// ストラクチャリストファイルへのパスを設定する
      /// </summary>
      /// <param name="Path">ストラクチャリストファイルへの相対パス</param>
      public void Load(string Path) => BVEMapHeader += "Structure.Load('" + Path + "');\n";
      /// <summary>
      /// 自軌道の指定の距離程を基準として指定の座標にストラクチャを設置する
      /// </summary>
      /// <param name="D">基準とする地点の距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="X">X座標[m]</param>
      /// <param name="Y">Y座標[m]</param>
      /// <param name="Z">Z座標[m]</param>
      /// <param name="Rx">軌道に対するX軸回りの角[deg]</param>
      /// <param name="Ry">軌道に対するY軸回りの角[deg]</param>
      /// <param name="Rz">軌道に対するZ軸回りの角[deg]</param>
      /// <param name="Tilt">
      /// <para>傾斜オプション</para><para> 0:常に水平, 1:勾配に連動, 2:カントに連動, 3:勾配とカントに連動</para></param>
      /// <param name="Span">曲線における弦の長さ[m]</param>
      public void Put(double D,string Str,double X,double Y,double Z,double Rx,double Ry,double Rz,double Tilt,double Span)=> Putforthis(D, Str, "0", X, Y, Z, Rx, Ry, Rz, Tilt, Span);
      /// <summary>
      /// 自軌道の指定の距離程を基準として指定の座標にストラクチャを設置する
      /// </summary>
      /// <param name="D">基準とする地点の距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="X">X座標[m]</param>
      /// <param name="Y">Y座標[m]</param>
      /// <param name="Z">Z座標[m]</param>
      /// <param name="Tilt">
      /// <para>傾斜オプション</para><para> 0:常に水平, 1:勾配に連動, 2:カントに連動, 3:勾配とカントに連動</para></param>
      /// <param name="Span">曲線における弦の長さ[m]</param>
      public void Put(double D, string Str, double X, double Y, double Z, double Tilt, double Span) => Putforthis(D, Str, "0", X, Y, Z, 0, 0, 0, Tilt, Span);

      /// <summary>
      /// 指定の軌道の指定の距離程の地点を基準としてストラクチャを設置する
      /// </summary>
      /// <param name="D">基準とする地点の距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN">基準とする軌道名</param>
      /// <param name="X">X座標[m]</param>
      /// <param name="Y">Y座標[m]</param>
      /// <param name="Z">Z座標[m]</param>
      /// <param name="Rx">軌道に対するX軸回りの角[deg]</param>
      /// <param name="Ry">軌道に対するY軸回りの角[deg]</param>
      /// <param name="Rz">軌道に対するZ軸回りの角[deg]</param>
      /// <param name="Tilt">
      /// <para>傾斜オプション</para><para> 0:常に水平, 1:勾配に連動, 2:カントに連動, 3:勾配とカントに連動</para></param>
      /// <param name="Span">曲線における弦の長さ[m]</param>
      public void Put(double D, string Str, string TN, double X, double Y, double Z, double Rx, double Ry, double Rz, double Tilt, double Span) => Putforthis(D, "'" + Str + "'", TN, X, Y, Z, Rx, Ry, Rz, Tilt, Span);
      /// <summary>
      /// 指定の軌道の指定の距離程の地点を基準としてストラクチャを設置する
      /// </summary>
      /// <param name="D">基準とする地点の距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN">基準とする軌道名</param>
      /// <param name="X">X座標[m]</param>
      /// <param name="Y">Y座標[m]</param>
      /// <param name="Z">Z座標[m]</param>
      /// <param name="Tilt">
      /// <para>傾斜オプション</para><para> 0:常に水平, 1:勾配に連動, 2:カントに連動, 3:勾配とカントに連動</para></param>
      /// <param name="Span">曲線における弦の長さ[m]</param>
      public void Put(double D, string Str, string TN, double X, double Y, double Z, double Tilt, double Span) => Putforthis(D, "'" + Str + "'", TN, X, Y, Z, 0, 0, 0, Tilt, Span);

      private void Putforthis(double D, string Str, string TN, double X, double Y, double Z, double Rx, double Ry, double Rz, double Tilt, double Span)
        => ListAdd(D, "Structure['" + Str + "'].Put(" + TN + ", "
          + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ", " 
          + Rx.ToString() + ", " + Ry.ToString() + ", " + Rz.ToString() + ", " + Tilt.ToString() + ", " + Span.ToString() + ")");

      /// <summary>
      /// 指定の軌道の指定の距離程の地点にストラクチャを設置する
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN">軌道名</param>
      /// <param name="Tilt">
      /// <para>傾斜オプション</para><para> 0:常に水平, 1:勾配に連動, 2:カントに連動, 3:勾配とカントに連動</para></param>
      /// <param name="Span">曲線における弦の長さ[m]</param>
      public void Put0(double D, string Str, string TN, double Tilt, double Span) => Put0forthis(D, Str, "'" + TN + "'", Tilt, Span);
      /// <summary>
      /// 自軌道の指定の距離程の地点にストラクチャを設置する
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="Tilt">
      /// <para>傾斜オプション</para><para> 0:常に水平, 1:勾配に連動, 2:カントに連動, 3:勾配とカントに連動</para></param>
      /// <param name="Span">曲線における弦の長さ[m]</param>
      public void Put0(double D, string Str, double Tilt, double Span) => Put0forthis(D, Str, "0", Tilt, Span);

      private void Put0forthis(double D, string Str, string TN, double Tilt, double Span) => ListAdd(D, "Structure['" + Str + "'].Put0(" + TN + ", " + Tilt.ToString() + ", " + Span.ToString() + ");");

      /// <summary>
      /// 指定の距離程に、自軌道と指定の軌道の間に沿うようにX軸方向に変形させながら、ストラクチャを設置する。
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN2">軌道名2</param>
      public void PutBetween(double D, string Str, string TN2) => PutBetween(D, Str, "0", "'" + TN2 + "'", "");
      /// <summary>
      /// 指定の距離程に、自軌道と指定の軌道の間に沿うように指定の方向へ変形させながら、ストラクチャを設置する。
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN2">軌道名2</param>
      /// <param name="Flag">変形方向(0:XY両方向, 1:X方向のみ)</param>
      public void PutBetween(double D, string Str, string TN2, int Flag) => PutBetween(D, Str, "0", "'" + TN2 + "'", ", " + Flag.ToString());
      /// <summary>
      /// 指定の距離程に、指定の2軌道間に沿うようにX軸方向に変形させながら、ストラクチャを設置する。
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN1">軌道名1</param>
      /// <param name="TN2">軌道名2</param>
      public void PutBetween(double D, string Str, string TN1, string TN2) => PutBetween(D, Str, "'" + TN2 + "'", "'" + TN2 + "'", "");
      /// <summary>
      /// 指定の距離程に、指定の2軌道間に沿うように指定の方向へ変形させながら、ストラクチャを設置する。
      /// </summary>
      /// <param name="D">距離程[m]</param>
      /// <param name="Str">ストラクチャ名</param>
      /// <param name="TN1">軌道名1</param>
      /// <param name="TN2">軌道名2</param>
      /// <param name="Flag">変形方向(0:XY両方向, 1:X方向のみ)</param>
      public void PutBetween(double D, string Str, string TN1, string TN2, int Flag) => PutBetween(D, Str, "'" + TN2 + "'", "'" + TN2 + "'", ", " + Flag.ToString());

      private void PutBetween(double D, string Str, string TN1, string TN2, string Flag) => ListAdd(D, "Structure['" + Str + "'].PutBetween(" + TN1 + ", " + TN2 + Flag.ToString() + ")");
    }

    /// <summary>
    /// ストラクチャの連続配置をするクラス=>未完
    /// </summary>
    public class Repeater
    {

    }
    /// <summary>
    /// 背景に関する設定を行うクラス=>未完
    /// </summary>
    public class Background
    {

    }
    /// <summary>
    /// 閉塞に関する設定を行うクラス=>未完
    /// </summary>
    public class Section
    {

    }
    /// <summary>
    /// 信号機に関する設定を行うクラス=>未完
    /// </summary>
    public class Signal
    {

    }
    /// <summary>
    /// 地上子の設置を行うクラス=>未完
    /// </summary>
    public class Beacon
    {

    }
    /// <summary>
    /// 速度制限を設定するクラス=>未完
    /// </summary>
    public class SpeedLimit
    {

    }
    /// <summary>
    /// 先行列車について設定するクラス=>未完
    /// </summary>
    public class PreTrain
    {

    }
    /// <summary>
    /// 光源に関する設定をするクラス=>未完
    /// </summary>
    public class Light
    {

    }
    /// <summary>
    /// 霧効果を設定するクラス=>未完
    /// </summary>
    public class Fog
    {

    }
    /// <summary>
    /// 風景描画距離を設定するクラス=>未完
    /// </summary>
    public class DrawDistance
    {

    }
    /// <summary>
    /// 運転台の明るさを設定するクラス=>未完
    /// </summary>
    public class CabIlluminance
    {

    }
    /// <summary>
    /// 軌道変位を設定するクラス=>未完
    /// </summary>
    public class Irregularity
    {

    }
    /// <summary>
    /// 粘着特性を設定するクラス=>未完
    /// </summary>
    public class Adhesion
    {

    }
    /// <summary>
    /// 音を設定するクラス=>未完
    /// </summary>
    public class Sound
    {

    }
    /// <summary>
    /// 固定音源を設定するクラス=>未完
    /// </summary>
    public class Sound3D
    {

    }
    /// <summary>
    /// 走行音を設定するクラス=>未完
    /// </summary>
    public class RollingNoise
    {

    }
    /// <summary>
    /// フランジ軋り音を設定するクラス=>未完
    /// </summary>
    public class FlangeNoise
    {

    }
    /// <summary>
    /// 分岐器通過音を設定するクラス=>未完
    /// </summary>
    public class JointNoise
    {

    }
    /// <summary>
    /// 他列車について設定するクラス=>未完
    /// </summary>
    public class Train
    {

    }

    /// <summary>
    /// Tetsu Otterが使いたいと思った機能を実装したクラス
    /// </summary>
    public class TR
    {
      /// <summary>
      /// TCLとCCLから曲線を設置する
      /// </summary>
      /// <param name="BTCDistance">BTCの距離程[m]</param>
      /// <param name="TCL">緩和曲線長[m]</param>
      /// <param name="CCL">円曲線長[m]</param>
      /// <param name="Radius">半径[m](正:右曲線 / 負:左曲線)</param>
      /// <param name="Cant">カント[m](半径と正負を一致させる)</param>
      public void TCLCCLtoCurve(double BTCDistance, double TCL, double CCL, double Radius, double Cant) => TCLCCLtoCurve(BTCDistance, TCL, CCL, TCL, Radius, Cant);
      /// <summary>
      /// 始端側TCL, 終端側TCLとCCLから曲線を設置する
      /// </summary>
      /// <param name="BTCDistance">BTCの距離程[m]</param>
      /// <param name="BTCL">始端側緩和曲線長[m]</param>
      /// <param name="ETCL">終端側緩和曲線長[m]</param>
      /// <param name="CCL">円曲線長[m]</param>
      /// <param name="Radius">半径[m](正:右曲線 / 負:左曲線)</param>
      /// <param name="Cant">カント[m](半径と正負を一致させる)</param>
      public void TCLCCLtoCurve(double BTCDistance, double BTCL, double CCL, double ETCL, double Radius, double Cant)
      {
        Curve c = new Curve();
        c.Interpolate(BTCDistance, 0, 0);
        c.Interpolate(BTCDistance + BTCL, Radius, Cant);
        c.Interpolate(BTCDistance + BTCL + CCL);
        c.Interpolate(BTCDistance + BTCL + CCL + ETCL, 0, 0);
      }

      /// <summary>
      /// 勾配標建植位置と勾配の大きさ、縦曲線半径を用いて自動で勾配の変化を作る
      /// <remarks><para>日本大百科全書(ニッポニカ)によると...</para>
      /// <para>普通鉄道の場合、縦R=2000(但し平面Rが600m以下の場合は縦R=3000)但し勾配変化が1000分の10以下の場合は省略可
      /// 新幹線の場合、縦R=10,000(但し最高250km/h以下の区間は縦R=5000)</para>
      /// </remarks>
      /// </summary>
      /// <param name="GradSignDist">勾配標建植位置の距離程[m]</param>
      /// <param name="StartGrad">始端側勾配[‰]</param>
      /// <param name="EndGrad">終端側勾配[‰]</param>
      /// <param name="Radius">縦曲線半径[m]</param>
      public void GradientChange(double GradSignDist, double StartGrad, double EndGrad, double Radius)
      {
        Gradient g = new Gradient();
        double Theta = Math.Atan((Math.Abs(StartGrad) + Math.Abs(EndGrad)) / 1000);
        double Long = Radius * Theta;
        Console.WriteLine("GradTransLong = " + Long.ToString());
        g.BeginTransition(GradSignDist - (Long / 2));
        g.Begin(GradSignDist + (Long / 2), EndGrad);
      }

      /// <summary>
      /// 1線から2線へ分岐する分岐器を設置する。(自線がカーブしている場合は不具合が発生する可能性あり)
      /// </summary>
      /// <param name="StartDist">分岐器開始位置[m]</param>
      /// <param name="CurveTrack">分岐側軌道情報</param>
      /// <param name="StraightTrack">直線側軌道情報</param>
      /// <param name="Num">分岐器の番数</param>
      /// <param name="IsCurveLeft">分岐側軌道が左方向に分岐するかどうか</param>
      /// <exception cref="ArgumentException">分岐器番数に0は指定できません。</exception>
      /// <exception cref="ArgumentException">分岐側軌道に自線は指定できません。</exception>
      /// <exception cref="ArgumentException">直線側軌道と分岐側軌道は同じにできません。</exception>
      /// <exception cref="ArgumentException">直線側軌道と分岐側軌道は同じ傾きである必要があります。</exception>
      public SWInfo One4TwoSWPut(double StartDist, TrackInfo CurveTrack, TrackInfo StraightTrack, bool IsCurveLeft, double Num) => One4TwoSWPut(StartDist, CurveTrack, StraightTrack, IsCurveLeft, Num, 1.067);
      /// <summary>
      /// 1線から2線へ分岐する分岐器を設置する。(自線がカーブしている場合は不具合が発生する可能性あり)
      /// </summary>
      /// <param name="StartDist">分岐器開始位置[m]</param>
      /// <param name="CurveTrack">分岐側軌道情報</param>
      /// <param name="StraightTrack">直線側軌道情報</param>
      /// <param name="Num">分岐器の番数</param>
      /// <param name="Gauge">軌間[m]</param>
      /// <param name="IsCurveLeft">分岐側軌道が左方向に分岐するかどうか</param>
      /// <exception cref="ArgumentException">分岐器番数に0は指定できません。</exception>
      /// <exception cref="ArgumentException">分岐側軌道に自線は指定できません。</exception>
      /// <exception cref="ArgumentException">直線側軌道と分岐側軌道は同じにできません。</exception>
      /// <exception cref="ArgumentException">直線側軌道と分岐側軌道は同じ傾きである必要があります。</exception>
      public SWInfo One4TwoSWPut(double StartDist, TrackInfo CurveTrack, TrackInfo StraightTrack, bool IsCurveLeft, double Num, double Gauge)
      {
        SWInfo sw = new SWInfo();
        Track t = new Track();
        if (CurveTrack.TrackName == "0") throw new Exception("分岐側軌道に自線を設定することはできません。", new ArgumentException());
        if (Num == 0) throw new Exception("分岐器番数に「０」を指定することはできません。", new ArgumentException());
        if (CurveTrack.TrackName == StraightTrack.TrackName) throw new Exception("直線側軌道と分岐側軌道は同じにできません。", new ArgumentException());
        if (CurveTrack.Slope != StraightTrack.Slope) throw new Exception("直線側軌道と分岐側軌道は同じ傾きである必要があります。", new ArgumentException());
        Num = Math.Abs(Num);
        double Theta = 2 * Math.Atan(1 / (2 * Num));
        double Alpha = Math.Atan(StraightTrack.Slope);
        double Radius = Gauge * ((1 / (1 - Math.Cos(Theta))) - 0.5);
        sw.SWLength = Radius * Math.Tan(Theta / 2) * (1 + Math.Cos(Theta) - Math.Sin(Theta) * Math.Tan(Alpha)) * Math.Cos(Alpha);
        if (IsCurveLeft) Radius = -Radius;
        if (CurveTrack.TrackName != "0") t.XInterpolate(StartDist, StraightTrack.TrackName, StraightTrack.Location[0], 0);
        t.XInterpolate(StartDist, CurveTrack.TrackName, CurveTrack.Location[0], Radius);
        sw.DistBetweenSAndC = Radius * Math.Tan(Theta / 2) * (1 + Math.Sin(Theta));
        sw.StartDistance = StartDist;
        sw.StraightEnd = StraightTrack;
        sw.CurveEnd = CurveTrack;
        sw.StraightEnd.Location[0] = StraightTrack.Location[0] + sw.SWLength * Math.Tan(Alpha);
        sw.CurveEnd.Location[0] = sw.StraightEnd.Location[0] + Radius * Math.Sin(Theta) * Math.Tan(Theta / 2) / Math.Cos(Alpha);
        if (IsCurveLeft) sw.CurveEnd.Slope -= 1 / Num;
        else sw.CurveEnd.Slope += 1 / Num;

        t.XInterpolate(sw.SWLength + StartDist, StraightTrack.TrackName, sw.StraightEnd.Location[0], 0);
        if (CurveTrack.TrackName != "0") t.XInterpolate(sw.SWLength + StartDist, CurveTrack.TrackName, sw.CurveEnd.Location[0], 0);

        return sw;
      }
      /// <summary>
      /// 他線に関する情報
      /// </summary>
      public class TrackInfo
      {

        /// <summary>
        /// TrackInfoを初期化
        /// </summary>
        /// <param name="TN">軌道名</param>
        public TrackInfo(string TN)
        {
          TrackName = TN;
        }
        /// <summary>
        /// TrackInfoを初期化
        /// </summary>
        /// <param name="TN">軌道名</param>
        /// <param name="LocationArray">軌道位置座標情報(x,y)[m]</param>
        public TrackInfo(string TN, double[] LocationArray)
        {
          TrackName = TN;
          Location = LocationArray.Take(2).ToArray();
        }
        /// <summary>
        /// TrackInfoを初期化
        /// </summary>
        /// <param name="TN">軌道名</param>
        /// <param name="LocationArray">軌道位置座標情報(x,y)[m]</param>
        /// <param name="SlopeNum">軌道の傾き情報(dz/dx)</param>
        public TrackInfo(string TN, double[] LocationArray, double SlopeNum)
        {
          TrackName = TN;
          Location = LocationArray.Take(2).ToArray();
          Slope = SlopeNum;
        }

        /// <summary>
        /// 軌道名
        /// </summary>
        public string TrackName;
        /// <summary>
        /// 軌道位置(座標(x,y,z))
        /// </summary>
        public double[] Location = new double[2];
        /// <summary>
        /// 自線に対する傾き(dz/dx)
        /// </summary>
        public double Slope;
      }
      /// <summary>
      /// 分岐器関数の処理結果
      /// </summary>
      public class SWInfo
      {
        /// <summary>
        /// 分機器のカーブ終了時の直線側と他線側との距離
        /// </summary>
        public double DistBetweenSAndC;
        /// <summary>
        /// 分岐器関数開始距離
        /// </summary>
        public double StartDistance;
        /// <summary>
        /// 分岐線処理終了時の分岐線側軌道の状態
        /// </summary>
        public TrackInfo CurveEnd;
        /// <summary>
        /// 直線側処理終了時の直線側軌道の状態
        /// </summary>
        public TrackInfo StraightEnd;
        /// <summary>
        /// 分岐器カーブ長さ(直線基準)
        /// </summary>
        public double SWLength;
      }

      /// <summary>
      /// 他線を自動で(力技で)真っすぐに配置してくれる便利な、とーっても便利な関数(TCLの半分とCCLの合計が90度以上の弧の長さに相当する場合は非対応)
      /// </summary>
      /// <param name="mci">自線の曲線情報</param>
      /// <param name="ti">真っすぐに配置したい他線の情報</param>
      /// <param name="Accurate">緩和曲線分割数[m](大きければ大きいほど直線に近くなります 0の場合は自動で50が代入されます)</param>
      public void KeepStraight(MyCurveInfo mci, ref TrackInfo ti, double Accurate)
      {
        if (Accurate < 0) throw new ArgumentOutOfRangeException("Accurate", Accurate.ToString(), "負の値を入れることはできません。");
        if (ti.TrackName == "0") throw new ArgumentException("他線に自線を指定することはできません。", "ti.TrackName");
        if (ti.TrackName == null) throw new ArgumentNullException("ti.TrackName", "他線の名前がnullです。");
        if (ti.TrackName == string.Empty) throw new ArgumentOutOfRangeException("ti.TrackName", "他線の名前が指定されていません。");
        if (Accurate == 0) Accurate = 50;
        double CutDistance = mci.STCL / Accurate;
        double NowZLocation = 0;
        double AbsR = Math.Abs(mci.Radius);

        double TCL = mci.STCL;
        double TCL2l2RPI2 = (TCL * TCL) / (2 * AbsR * Math.PI * Math.PI);
        double NowX = Calc(0);


        double Calc(double Nowz)
        {
          return (Nowz * Nowz / (4 * mci.Radius)) - TCL2l2RPI2 * (1 - Math.Cos(Nowz * Math.PI / TCL));
        }
      }

      /// <summary>
      /// 自線の曲線情報についてのクラス
      /// </summary>
      public class MyCurveInfo
      {
        /// <summary>
        /// MyCurveInfoを曲線情報変化位置情報を用いて初期化する
        /// </summary>
        /// <param name="R">円曲線の半径[m]</param>
        /// <param name="BTC">緩和曲線開始位置[m]</param>
        /// <param name="BCC">円曲線開始位置[m]</param>
        /// <param name="ECC">円曲線終了位置[m]</param>
        /// <param name="ETC">緩和曲線終了位置[m]</param>
        public MyCurveInfo(double R, double BTC, double BCC, double ECC, double ETC)
        {
          Radius = R;
          BTCDistance = BTC;
          BCCDistance = BCC;
          ECCDistance = ECC;
          ETCDistance = ETC;
          STCL = BCC - BTC;
          CCL = ECC - BCC;
          ETCL = ETC - ECC;
        }

        internal double Radius;
        internal double BTCDistance;
        internal double BCCDistance;
        internal double ECCDistance;
        internal double ETCDistance;
        internal double STCL;
        internal double CCL;
        internal double ETCL;
      }

      /// <summary>
      /// 他線を設定するクラス
      /// </summary>
      public class TrackCreate
      {
        static private List<string> TrackNameList = new List<string>();
        private string TrackName { get; }
        private List<double[]> Location = new List<double[]>();//Z,X,Y

        /// <summary>
        /// 他線を新しく定義する
        /// </summary>
        /// <param name="Name">軌道名</param>
        /// <param name="X">初期X座標[m]</param>
        /// <param name="Y">初期Y座標[m]</param>
        /// <param name="Gauge">軌間[m]</param>
        public TrackCreate(string Name, double X, double Y, double Gauge)
        {
          if (Name == null) throw new ArgumentNullException("Name", "軌道名にnullを指定することはできません。");
          else if (Name == string.Empty) throw new ArgumentException("軌道名がEmptyです。", "Name");
          else if (Name == "0") throw new ArgumentException("軌道名「0」は自軌道を表す文字として予約されています。", "Name");
          else
          {
            Exception e = null;
            try
            {
              TrackNameList.Find(n => n == Name);
            }
            catch (ArgumentNullException ex)
            {
              e = ex;
              TrackName = Name;
              TrackNameList.Add(Name);
            }
            finally
            {
              if (e == null) throw new ArgumentException("軌道名'" + Name + "'は既に使用されています。", "Name");
            }
          }

        }

      }
    }


    /// <summary>
    /// OutputListに構文を追加する
    /// </summary>
    /// <param name="Distance">距離程</param>
    /// <param name="Syntax">構文</param>
    static private void ListAdd(double Distance, string Syntax)
    {
      int Ind = 0;
      try
      {
        Ind = Output.FindIndex(OutputList => OutputList.Distance == Distance);
      }
      catch (ArgumentNullException) { Output.Add(new OutputList(Distance, Syntax)); return; }
      if (Ind< 0)
      {
        Output.Add(new OutputList(Distance, Syntax));
      }
      else
      {
        Output[Ind].Syntax.Add(Syntax);
      }
    }
  }
}
