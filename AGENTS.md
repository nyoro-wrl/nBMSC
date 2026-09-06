# リリース

- 既存の Release ワークフローは `master` への push で実行させる
- `.github/workflows/release.yml` は、ワークフロー自体が壊れている場合、またはユーザーがリリース方式の変更を明示した場合だけ修正する
- バージョン更新は原則として以下だけを変更する
  - `src/My Project/AssemblyInfo.vb` の `AssemblyVersion` / `AssemblyFileVersion`
  - `src/nBMSC.vbproj` の `ApplicationVersion`
- CI/CD は配布物を添付した下書き（draft）の Release を作成し、公開しない
- CI/CD 成功後、確定した本文を `gh release edit <version> --repo nyoro-wrl/nBMSC --notes-file <file> --draft=false` で設定すると同時に公開する。本文が空の状態で公開しない
- Release 本文が指定されていない場合は確認する

# WinForms UI

- UI配置変更は原則として既存の `*.Designer.vb` / `.resx` の構造を優先する
- Visual Studio Designerでの編集が必要な場合は、コード側へ後付け実装せず確認する
- 実行時生成が既存パターンの画面に限り、コード側でのUI生成を許可する
