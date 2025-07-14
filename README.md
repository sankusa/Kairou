# Kairou
Unityで会話イベント等の順次的なイベントを作成することができる、イベント構築基盤ライブラリです。
- Commandパターンに基づいており、コマンドを並べていくことでイベントを構築することができます
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
1. Releaseぺージから.unitypackageをダウンロードする
2. 導入したいプロジェクトに.unitypackageをインポートする

## 使い方

## License
MIT License
