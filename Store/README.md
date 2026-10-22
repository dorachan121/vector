# Google Play で公開するまでの手順

VECTOR を Google Play に出すための作業をまとめています。アプリの設定と、ストアに載せる素材はこのリポジトリにそろっています。

| 項目 | 値 |
|---|---|
| パッケージ名 | `com.dorachan121.vector`（公開後は変更できない） |
| バージョン | 1.0.0（Version Code 1） |
| 対象 API レベル | 36（Android 16）。2026年8月31日以降、新しいアプリと更新に必須 |
| 最小 API レベル | 26（Android 8.0） |
| CPU | ARM64（IL2CPP） |
| 権限 | なし（インターネットにも接続しない） |
| 形式 | Android App Bundle（.aab） |

## 1. アップロード鍵を作る（最初の1回だけ）

Play に上げる .aab は、自分の鍵（アップロード鍵）で署名します。**鍵ファイルとパスワードはリポジトリに入れず、なくさないように保管してください。**
（Play App Signing を使うので、アプリ本体の署名鍵は Google が管理します。アップロード鍵をなくしても Play Console から再発行を申し込めます。）

PowerShell で次を実行し、聞かれるパスワードと名前を入力します。

```powershell
New-Item -ItemType Directory -Force "$env:USERPROFILE\keys"
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe" `
  -genkeypair -v -keystore "$env:USERPROFILE\keys\vector-upload.jks" `
  -alias upload -keyalg RSA -keysize 2048 -validity 10000
```

## 2. .aab を作る

鍵の場所とパスワードを環境変数で渡してから、Unity のメニュー **Vector/Build Android App Bundle** を実行します。
`Builds/Android/VECTOR-<バージョン>-<Version Code>.aab` ができます。

```powershell
$env:VECTOR_KEYSTORE = "$env:USERPROFILE\keys\vector-upload.jks"
$env:VECTOR_KEYSTORE_PASS = "（キーストアのパスワード）"
$env:VECTOR_KEY_ALIAS = "upload"
# 鍵のパスワードをキーストアと別にしたときだけ
# $env:VECTOR_KEY_PASS = "（鍵のパスワード）"
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe" -projectPath "C:\UnityProjects\Vector\vector"
```

環境変数を使わない場合は、Player Settings → Publishing Settings で鍵を選んでパスワードを入れてからメニューを実行しても構いません（パスワードは保存されません）。
エディタを閉じた状態なら、バッチモードでも作れます。

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\UnityProjects\Vector\vector" `
  -executeMethod Vector.EditorTools.AndroidRelease.Build -logFile build.log
```

**2回目以降に Play に上げるときは、Player Settings の Version（例 1.0.1）と Bundle Version Code（2, 3, …）を必ず上げてください。** 同じ Version Code は上げられません。

## 3. Play Console でアプリを作る

1. [Play Console](https://play.google.com/console) で「アプリを作成」。アプリ名 `VECTOR カジノリゾートRPG`、デフォルトの言語は日本語、「ゲーム」「無料」を選ぶ
2. 「ストアの掲載情報」に [listing-ja.md](listing-ja.md) の文面と、このフォルダの画像を登録する
   - アイコン `icon-512.png`、フィーチャーグラフィック `feature-graphic.png`、スクリーンショット `screenshots/` の8枚
3. 「アプリのコンテンツ」を埋める（下の表）

| 項目 | 答え |
|---|---|
| プライバシーポリシー | `https://github.com/dorachan121/vector/blob/master/Store/privacy-policy.md` |
| 広告 | 広告を含まない |
| アプリのアクセス権 | すべての機能を制限なしで利用できる |
| コンテンツのレーティング | 質問票で「シミュレーションのギャンブル」があると答える（本物のお金は賭けない・景品はない・ユーザー同士のやりとりはない・デジタル購入はない） |
| ターゲット ユーザー | **18歳以上だけ**を選ぶ（カジノを模したゲームは子ども向けにしない） |
| データ セーフティ | 「ユーザーデータを収集・共有しない」。データは端末の外に出ない |
| 政府・金融・健康などの機能 | なし |

## 4. クローズドテスト（個人アカウントでは必須）

2023年11月13日より後に作った個人の開発者アカウントは、本番に出す前に**12人以上のテスターが14日間続けて参加したクローズドテスト**が必要です。

1. 「テスト → クローズドテスト」でトラックを作り、.aab を上げる
2. テスターのメールアドレス（Google アカウント）を12人以上（抜ける人に備えて15人ほど）登録する
3. 参加用のリンクをテスターに送り、テストに参加して（オプトインして）もらう
4. 14日間続けて12人以上が参加したら、ダッシュボードから「本番環境へのアクセスを申請」する。テストで得た意見なども質問される

## 5. 本番に公開する

本番環境へのアクセスが認められたら、「本番環境 → 新しいリリースを作成」で .aab を上げ、国や地域を選んで審査に出します。
審査には数日かかることがあります。

## 素材を作り直すとき

- アイコンとフィーチャーグラフィック：Unity のメニュー **Vector/Build Store Art**（Android のアダプティブアイコンも登録し直す）
- スクリーンショット：縦 1080×1920・横 1920×1080 で撮り直す（Play の規定は縦横比 2:1 まで）
