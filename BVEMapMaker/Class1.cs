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
    static string BVEMapHeader;
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
      /// <param name="X">X軸方向位置[m]</param>
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
