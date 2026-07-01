# nBMSC 作業ルール

## リリース

- 既存の Release ワークフローは `master` への push で実行させる
- `.github/workflows/release.yml` は、ワークフロー自体が壊れている場合だけ修正する
- バージョン更新は原則として以下だけを変更する
  - `src/My Project/AssemblyInfo.vb` の `AssemblyVersion` / `AssemblyFileVersion`
  - `src/nBMSC.vbproj` の `ApplicationVersion`
- コミット・push 前にローカルで Release ビルドを確認する
- Release 本文は CI/CD で Release が作成された後、`gh release edit <version> --notes-file <file>` で設定する
- Release 本文が指定されていない場合は確認する
