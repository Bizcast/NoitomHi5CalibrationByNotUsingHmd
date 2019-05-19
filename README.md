# NoitomHi5CalibrationByNotUsingHmd
Noitom Hi5をHMD無しでもキャリブレーションすることができるプロジェクト。  
![demo](Documents/demo.gif)

## 概要
公式から出されているNoitom Hi5の[キャリブレーション用プロジェクト](https://hi5vrglove.com/downloads/unity)は、HMDを被って操作する必要があります。  
またHMDでの操作が必要なことから、VR Readyな環境でなければキャリブレーションできないという問題もあります。  
これは運用の面から考えると非常に面倒なものです。  


そこでキーボード入力によって操作を行えて、かつVR Readyではない環境でも疑似的なキャリブレーションを行えるようなプロジェクトを作成しました。  
Noitom Hi5のキャリブレーションには本来Vive Controller / Vive Trackerが必要ですが、VR Readyではない環境の場合これらを使用することができません。  
その為、Vive Controller / Vive Trackerの動き（以下、OpticalData）を事前に記録しておき、キャリブレーション時に読み込むことで疑似的にキャリブレーションできるようにしています。

## 使い方
### 事前準備
1. [Steam](https://store.steampowered.com/)から[SteamVR](https://store.steampowered.com/app/250820/SteamVR/)をインストールする ~~（VR Readyではない環境でも必須です）~~。
1. 本リポジトリをクローンしてサンプルシーンを表示する、または[Releases](https://github.com/Bizcast/NoitomHi5CalibrationByNotUsingHmd/releases)からバイナリをダウンロードする。
1. 本リポジトリをクローンした場合は、[Noitom Hi5 Unity SDK](https://hi5vrglove.com/downloads/unity)と[SteamVR Plugin](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)をダウンロードしてインポートする。

### VR Readyな環境の場合
4. サンプルシーンを再生するかバイナリを実行する。
4. Vive Controller / Vive Trackerが接続されていることを確認し、**F2キー** を押下してキャリブレーションを開始する。

### VR Readyではない環境の場合
4. 事前にVR Readyな環境でキャリブレーションを行い新規でOpticalDataを生成する。もしVR Readyな環境が無い場合はバイナリに同梱しているOpticalDataを用意する。
4. 新規で作成したOpticalDataを使用する場合は、`C:\Users\（ユーザ名）\AppData\Roaming\HI5` に生成された `OpticalDeviceBindInfo.xml` ファイルを開き、2組あるHandTypeとSerialNumberを確認する。  
  同梱しているOpticalDataを使用する場合は、`C:\Users\（ユーザ名）\AppData\Roaming\HI5\OpticalData` にOpticalDataをコピーし 5. に進む。
4. `C:\Users\（ユーザ名）\AppData\Roaming\HI5\OpticalData` に生成されたOpticalDataのファイル名が 3. で確認したSerialNumberになっているはずなので、そのSerialNumberと対応するHnadTypeにファイル名を変更する。  
ファイル名は必ず `LEFT.csv`、`RIGHT.csv` のどちらかになります。
4. サンプルシーンを再生するかバイナリを実行する。
4. **F1キー** を押下して **疑似** キャリブレーションを開始する。

### 共通
9. Bポーズ、Pポーズキャリブレーションを画面の指示に従って行う。
9. キャリブレーション完了後、 `C:\Users\（ユーザ名）\AppData\Roaming\HI5` 以下にキャリブレーションデータ及びOpticalDataが生成されるため、必要ならばバックアップを取る。
9. エンジョイ！

## 動作環境
- Unity2018.2.21f1
- Noitom Hi5 Unity SDK v1.0.0.655.16
- SteamVR Plugin v2.2.0  
  VR Readyではない環境でも必須です。

## FAQ
### 疑似キャリブレーションが始まらないんだけど
画面下部にエラーメッセージを出力するようにしています。そのメッセージに従ってください。  
よくあるミスとしては、OpticalDataのファイル名を `LEFT.csv`、`RIGHT.csv` に変更することを忘れることです。

### 疑似キャリブレーションを行ってもキャリブレーション結果が悪いんだけど
あくまでも疑似キャリブレーションであるため、記録されたOpticalDataと同一の位置/方向で、同一の動きをしなければキャリブレーション結果が悪くなることを確認しています。  
**新規で作成したOpticalDataを使用している場合** は、位置/方向をそのデータを記録した位置/方向と同一にし、腕の移動開始タイミング/スピードを同一にすることで改善される可能性があります。  
**同梱しているOpticalDataを使用している場合** は、使用するスペースの中心/正面方向に立ち、腕の移動開始タイミング/スピードは画面上のアニメーションと同一にすることで改善される可能性があります。

## ライセンス
本リポジトリは[MITライセンス](https://github.com/Bizcast/NoitomHi5CalibrationByNotUsingHmd/blob/master/LICENSE)の下で公開しています。
