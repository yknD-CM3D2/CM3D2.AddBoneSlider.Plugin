## CM3D2.AddBoneSlider.Plugin
CM3D2のメイドさんのボーンなどをGUIで操作する UnityInjector 用プラグイン
![GUI](http://i.imgur.com/oK0XFke.jpg "GUI")

### これは何？
　CM3D2のメイドさんのボーンなどをGUIで動かすプラグインです。  
　動かしたボーンはポーズとして保存・anm出力(1フレームのみ)できます。  
　エディットモード、公式撮影モードで動作します。

## 導入方法
Ver 1.30以降  
ソースをコンパイルしたもの、もしくは[作者のアップローダ][]にある最新版の  
CM3D2.AddModsSlider.Plugin.dllをUnityInjector以下に保存  
また、UnityInjector\config下にBoneParam.xmlも配置する

## 使い方
エディットモードおよび公式撮影モードにてF10キー（変更可）を押すとGUIが起動します。  
なお、公式撮影モードではメイドを1人以上呼び出さないとGUIを起動できません。  
GUIの使い方については図説説明書をごらんください。(※Cはクリックの略です)  
iniファイルや各種ポーズ保存用のファイル・フォルダは自動生成されます。  

### iniファイルの設定項目について
プラグイン起動後、UnityInjector\Config内にboneslider.iniファイルが生成されます。  
それぞれの設定項目は以下の通りです。  

* ToggleKey：GUI起動キー。デフォルトはF10。大文字記述でも小文字記述でもどちらでも大丈夫です。  
* PoseXMLDirectory：保存ポーズ情報(bonepose.xml)の保存場所。デフォルトは"UnityInjector\Config"
* PoseImgDirectory：保存ポーズサムネイルの保存場所。デフォルトは"UnityInjector\Config\PoseImg"
* OutputANMDirectory：anmファイル出力場所。
　デフォルトは"C:\KISS\CM3D2\PhotoModeData\Mod\Motion"
* DebugLogLevel：ログ出力レベル。デフォルトは0。
　デバッグ用で1以上にするとコンソールがぐちゃぐちゃになります。
* WindowPositionX/Y：GUIパネルの位置。デフォルトはそれぞれ0。
　パネルが画面外に行ってドラッグできなくなったときは0に戻してください。

### ポーズ保存機能について
 Saveボタンを左クリックすると現在のポーズが保存され、ポーズパネルに登録されます。  
 ポーズ情報はbonepose.xmlに保存され、サムネイルは上記のフォルダに保存されます。  
 ポーズを呼び出す場合はポーズパネルの該当ポーズを左クリック、  
 ポーズを削除する場合は右クリックします。  
 また、Saveボタンを右クリックすると上記のフォルダに現在のポーズを元にしたanmファイルが出力されます。  
 なお、出力されるanmファイルの名前は出力時のPC時刻を元に設定しています。  


### 出力したanmファイルについて
 出力したanmファイルは以下の方法でCM3D2本体に読み込ませることができます。
 
1. 公式撮影モード用フォルダに保存  
PhotoModeData\Mod\Motionフォルダ内にanmファイルを置くと  
公式撮影モードの[Mod]カテゴリーのモーションとして読み込まれます。  
公式撮影モードでしか使用できませんが、anmファイルを移動するだけで読み込むことができます。  
詳しくは同フォルダ内にあるReadme.txtをご覧ください。  
ファイル名がそのままモーション名になるので分かりやすいようリネームしてください。  

2. しばりすフォルダ内に保存  
ポーズ表示用画像(80*80ピクセル)をtexで準備し、  
Sybaris\GameData内のどこかにanmファイルとtexファイルを保存してください。  
また、ポーズ情報をjsonファイルとして用意してSybaris\Poses内に保存してください。  
詳しくはjsonファイルを適当なテキストエディターで開いて確認してください。  
各種ファイルの準備が必要ですが、エディットモードでもポーズを呼び出すことができます。  
texファイルの出力方法やjsonファイルの設定項目についてはここでは割愛します。  
詳細はしばりすreadme.txtの## FAQ ををご覧ください。  

### ALLPOSとOFFSETとBip01の違いについて
 ALLPOSは画面上の全てのメイドの位置を動かします。  
 OFFSETは対象のメイドだけ位置を動かします。  
 上記二つはポーズ保存やanm出力で保存されません。  
 Bip01も対象のメイドの位置を動かしますが、  
 こちらはポーズ保存やanm出力で位置情報が保存されます。  

### 左手/右手IKについて
 このボーンはメイドアイテムがアタッチされるボーンです。モップやワイングラスなどを持たせたとき、  
 モーションによってずれる場合はこのボーンを回転させると正しく持たせることができるかもしれません。  
 なお、一部のモーションは左手/右手IKの値が設定されており、（ドキドキ Fallin' Loveなど）  
 それらのモーション中はメイドアイテムの回転調整はできません。  

### 歩幅（Bip01 Footsteps）について
 不明です。動かしても特に何か動く訳じゃないみたいです。

## 注意事項
1. まだα版テストを終えたばかりのβ版です。バグ・不具合・UIの糞さの報告は豆腐を扱うかのごとくお願いします。  
2. スライダーでボーンを動かすとき、まれにジンバルロックを起こしスライダーが挙動不審になるときがあります。  
その場合は角度を少し変えて動かすかハンドルでボーンを動かしてみてください。  
3. ハンドルは現在ボーンの回転操作のみしか操作できません。その他カテゴリのパネルにも表示されますが、無反応です。  
4. スライダーの刻み幅が大きい場合、微調整する設定が現在ありません。入力ボックスに直に値を入力して微調整してください。  
5. ボーンの可動範囲は実際の人体の関節の可動範囲を参考にして設定してあります。  
それでも可動範囲が狭いと感じたら、Config内のBoneParam.xmlの該当ボーンのmax、minの値を大きくしてみてください。  
6. スライダーとハンドルで回転の仕方が異なります。前者は軸が動きませんが、後者は動きます。  
そのため、ハンドルを軸一つで動かしても、スライダー側では2,3軸動くことがあります。  
どれかの軸が稼働限界までいくとハンドルもそれ以上動かなくなるので  
その場合はスライダー側で限界になっている軸の値を少し下げてみてください。  
7. 出力されるanmファイルは1フレームのみの静止ポーズです。1フレーム以上のモーション指定に関しては  
現在自分の技術の考慮に入れながらUIを検討しています。M○DやM○MみたいなUIは期待しないでください。  
8. スライダーパネルを大量に開くとその分動作が重くなっていきます。  
　操作しないパネルがあればこまめに閉じるようにしてください。  
9. ポーズ保存機能およびanm出力はその他カテゴリ以外のボーンが対象です。  
　カメラや目、胸などは保存されないので注意してください。  
10. スクリーンショット撮影時、GUIパネルは他の標準UIに準じて写りますが、  
ハンドルはオブジェクトとしてUI設定関係なく写り込みます。  
スクリーンショット撮影時はハンドルを消してから撮影するようにしてください。  
あえてメイドさんがハンドル君と戯れるSSを撮影してみるのも面白いかもしれません。  
11. 複数メイド撮影プラグインとの重大な競合は（今のところ）確認されていません。  
ただし、複数メイド撮影プラグインで胸を動かすと本プラグイン側で操作できなくなります。  
また、複数メイド撮影プラグインを閉じた後にスカートボーンが反応しなくなりますが、  
これは複数メイド撮影プラグインの仕様です。  
また、CameraUtility、Cameraworkプラグインとの競合も現在確認されていません。  
12. 改変および改変物の再配布、アドオン・プラグインへの取り込みなどは自由です。  
むしろ改変できる人がいればじゃんじゃん改変してください。


## 更新履歴

###Ver0.0.1.1a
* 複数メイド撮影モードで13人目以降に雇ったメイドがパネルで呼び出せない問題を修正

###Ver0.0.1.1 
* だいたいバグが取れた（と思う）のと事情の変化によりオープンβ版として公開

###Ver0.0.0.x 
* 初版が完成するも諸事情により公開見送り＆個人ツールとして開発継続


##【利用規約】
**KISS** :  
* MODはKISSサポート対象外です。  
* MODを利用するに当たり、問題が発生しても製作者・KISSは一切の責任を負いかねます。  
* カスタムメイド3D2を購入されている方のみが利用できます。  
* カスタムメイド3D2上で表示する目的以外の利用は禁止します。  

**夜勤D** :    
* 再配布＆無断転載禁止。  
* 改造・改変、それに伴う再配布などはご自由にどうぞ

# 謝辞
* 本プラグインを開発するにあたり、[CM3D2.AddModsSlider.Plugin][]をベースにさせていただきました。  
また、ボーン操作部分は[CM3D2.IKCMOController.Plugin][]も参考にさせていただきました。  

　両作者様にこの場を借りてお礼申し上げます。

[CM3D2.AddModsSlider.Plugin]: https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin "CM3D2-01/CM3D2.AddModsSlider.Plugin/"
[CM3D2.IKCMOController.Plugin]:https://github.com/CM3D2-01/CM3D2.IKCMOController.Plugin "CM3D2-01/CM3D2.IKCMOController.Plugin "
[作者のアップローダ]:http://ux.getuploader.com/yakinD/ "夜勤の巣"

