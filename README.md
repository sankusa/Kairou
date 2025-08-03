# Kairou
Unityで会話イベント等の順次的なイベントを作成することができる、イベント構築基盤ライブラリです。
- Commandパターンで実装されており、コマンドを並べていくことでイベントを構築することができます
- 待機やフロー制御、ログ出力等の基本的なコマンドは最初から実装されています
- 会話やアニメーション等のゲームコンテンツに関わる処理は、カスタムコマンドとして利用者自身が実装する必要があります

## 依存アセット
本アセットを導入する前に、下記のアセットを導入している必要があります。
- [UniTask](https://github.com/Cysharp/UniTask)

## 外部アセット連携
### DIコンテナ系
依存性の注入やDIコンテナをコマンド内で利用できます。
- [VContainer](https://github.com/hadashiA/VContainer)
- [Zenject](https://github.com/Mathijs-Bakker/Extenject)

## インストール
### .unitypackageのインポート
1. Releaseぺージから.unitypackageをダウンロード
2. 導入したいプロジェクトに.unitypackageをインポート

## 使い方
Kairouでは、ScriptBookというオブジェクトを管理単位としてイベントを構築します。  
ScriptBookはアセット形式・コンポーネント形式があり、相互変換可能(予定)です。
### ScriptBookの作成
※シーン内のオブジェクトを参照する場合はコンポーネント形式である必要があります。  
- アセット形式の場合、Create→Kairou→ScriptBookAsset  
- コンポーネント形式の場合、AddComponent→ScriptBookComponent  

### ScriptBookの編集
アセット/コンポーネントのインスペクタからScriptBookEditorを開くことで編集ができます。
<img width="914" height="551" alt="image" src="https://github.com/user-attachments/assets/e97a5338-f669-478b-914a-647251cc5e38" />

### ScriptBookの基本構造
ScriptBook  
├ Page0  
│ ├ Command0  
│ ├ Command1  
│ ├ ...  
├ Page1  
│ ├ Command0  
│ ├ Command1  
│ ├ ...  
├ Page2  
├ ...  

上図のように、ScriptBookは内部に複数のページを持ち、各ページは内部に複数のコマンドを持ちます。  
<b>ScriptBookは実行時、最初のページのみ処理されます。</b>  
他のページを呼び出したい場合はページ呼び出し機能を持つコマンドを使う必要があります。  

### ScriptBookのその他の項目
#### Id
ScriptBookのIdは基本的には入力不要です。Idを使用してScriptBookを呼び出す機能を使用する場合には、一意となるIdを定める必要があります。

### Pageのその他の項目
#### Id
PageのIdは、ページ呼び出し時に必要になります。自動的に呼び出されるエントリーぺージ(0ページ目)以外は、ScriptBook内で一意となるIdを定める必要があります。

### ScriptBookの実行
1. GameObjectにScriptBookEngineをアタッチ
2. BookSlotsに要素を追加し、ScriptBookの種類(アセット/コンポーネント)に応じたTypeを選択
3. ScriptBookをアサイン
4. RunOnStartにチェックを入れている場合、実行時に自動でScriptBookが実行される
5. RunOnStartにチェックを入れていない場合、別のコンポーネント等からScriptBookEngineのRunメソッドを呼び出す

## License
MIT License
