namespace UbiquitousLanguageManager.Domain.Common

// 🎯 CommonSpecifications.fs: 仕様パターン（Specification Pattern）
// Bounded Context: Common
// このファイルは全Bounded Contextで共有される共通仕様パターンを定義します
//
// 【F#初学者向け解説】
// 仕様パターン（Specification Pattern）は、複雑なビジネスルールを
// 型安全で組み合わせ可能な形で実装するためのデザインパターンです。
//
// 主な利点:
// 1. ビジネスルールの再利用: 同じ条件を複数箇所で使える
// 2. 組み合わせ可能: AND/OR/NOTで複雑な条件を構築
// 3. テスト容易性: 個別の仕様を独立してテスト可能
// 4. 可読性: ビジネスルールが明示的な型として表現される

// 🔧 仕様の基底型: すべての仕様が実装すべきインターフェース
// 【F#初学者向け解説】
// ISpecification<'T> は「ジェネリック型パラメータ 'T」を取ります。
// 'T（シングルクォート付き）はF#のジェネリック型の記法です。
// C#の ISpecification<T> に相当します。
//
// abstract member: インターフェースの抽象メソッド定義
// - IsSatisfiedBy: 指定されたオブジェクトが仕様を満たすか判定
// - GetReasonForFailure: 満たさない場合の理由を返す（Option型）
type ISpecification<'T> =
    // 仕様判定メソッド: 'T型のオブジェクトがこの仕様を満たすか
    // 【F#初学者向け解説】
    // -> bool は「この関数はbool型の値を返す」という意味です。
    // 矢印（->）は関数の型シグネチャで使われます。
    abstract member IsSatisfiedBy: 'T -> bool

    // 失敗理由取得メソッド: 仕様を満たさない場合の理由
    // 【F#初学者向け解説】
    // string option は「文字列があるかもしれないし、ないかもしれない」を型で表現。
    // - Some "理由": 理由がある場合
    // - None: 理由がない場合（または仕様を満たしている場合）
    // これによりnullを使わずに「値が存在しないこと」を安全に表現できます。
    abstract member GetReasonForFailure: 'T -> string option

// 🔧 仕様の組み合わせ演算子: 複数の仕様を論理演算で結合
// 【F#初学者向け解説】
// moduleはF#の名前空間のようなもので、関連する型と関数をグループ化します。
// C#のstatic classに近い概念です。
module SpecificationCombiners =

    // 🤝 AND仕様: 両方の仕様を満たす必要がある
    // 【F#初学者向け解説】
    // type AndSpec<'T>(...) = ... は「クラス定義」です。
    // コンストラクタで2つのISpecification<'T>を受け取り、
    // 両方を満たす場合のみ成功する新しい仕様を作ります。
    //
    // 使用例:
    //   let spec = AndSpec(spec1, spec2)
    //   spec.IsSatisfiedBy(item)  // spec1とspec2の両方がtrueの場合のみtrue
    type AndSpec<'T>(left: ISpecification<'T>, right: ISpecification<'T>) =
        interface ISpecification<'T> with
            // && は論理AND演算子（C#と同じ）
            member _.IsSatisfiedBy(item: 'T) =
                left.IsSatisfiedBy(item) && right.IsSatisfiedBy(item)

            // 【F#初学者向け解説】
            // match式は複数の条件分岐を扱う強力な構文です。
            // タプル (value1, value2) を使って2つの値を同時にマッチングします。
            //
            // パターンの読み方:
            // - None, None: 両方の仕様が満たされている（理由なし）
            // - Some reason, None: 左の仕様が失敗
            // - None, Some reason: 右の仕様が失敗
            // - Some left, Some right: 両方失敗（両方の理由を連結）
            member _.GetReasonForFailure(item: 'T) =
                match left.GetReasonForFailure(item), right.GetReasonForFailure(item) with
                | None, None -> None
                | Some reason, None | None, Some reason -> Some reason
                | Some leftReason, Some rightReason -> Some $"{leftReason}、{rightReason}"

    // 🔀 OR仕様: どちらかの仕様を満たせば良い
    // 【F#初学者向け解説】
    // どちらか一方の仕様を満たせば成功する仕様です。
    // || は論理OR演算子（C#と同じ）
    //
    // 使用例:
    //   let spec = OrSpec(spec1, spec2)
    //   spec.IsSatisfiedBy(item)  // spec1またはspec2のどちらかがtrueの場合true
    type OrSpec<'T>(left: ISpecification<'T>, right: ISpecification<'T>) =
        interface ISpecification<'T> with
            member _.IsSatisfiedBy(item: 'T) =
                left.IsSatisfiedBy(item) || right.IsSatisfiedBy(item)

            member _.GetReasonForFailure(item: 'T) =
                // どちらかが満たされていれば成功（理由なし）
                if left.IsSatisfiedBy(item) || right.IsSatisfiedBy(item) then
                    None
                else
                    // 両方とも満たされていない場合のみエラー
                    Some "いずれの条件も満たしていません"

    // 🚫 NOT仕様: 仕様を満たさない場合に成功
    // 【F#初学者向け解説】
    // 既存の仕様を反転する仕様です。
    // 元の仕様がfalseの場合にtrueを返します。
    //
    // 使用例:
    //   let spec = NotSpec(activeUserSpec)
    //   spec.IsSatisfiedBy(user)  // ユーザーが非アクティブの場合true
    type NotSpec<'T>(spec: ISpecification<'T>) =
        interface ISpecification<'T> with
            // not は論理否定演算子（C#の ! に相当）
            member _.IsSatisfiedBy(item: 'T) =
                not (spec.IsSatisfiedBy(item))

            member _.GetReasonForFailure(item: 'T) =
                if spec.IsSatisfiedBy(item) then
                    Some "条件を満たすべきではありません"
                else
                    None

// 🎯 仕様の利用例とヘルパー関数
// 【F#初学者向け解説】
// 仕様パターンを使いやすくするヘルパー関数群です。
// 仕様をResult型と組み合わせて、Railway-oriented Programmingを実現します。
module SpecificationHelpers =

    // 🔧 仕様を適用してResult型で結果を返すヘルパー
    // 【F#初学者向け解説】
    // ISpecification<'T>を受け取り、'T型のアイテムに適用し、
    // Result<'T, string>を返します。
    //
    // Result型は以下の2つの状態を表現:
    // - Ok item: 仕様を満たす場合、元のアイテムをそのまま返す
    // - Error reason: 仕様を満たさない場合、理由を返す
    //
    // 使用例:
    //   let result = applySpecification activeUserSpec user
    //   match result with
    //   | Ok validUser -> (* 有効なユーザーで処理続行 *)
    //   | Error reason -> (* エラーメッセージを表示 *)
    let applySpecification<'T> (spec: ISpecification<'T>) (item: 'T) : Result<'T, string> =
        if spec.IsSatisfiedBy(item) then
            Ok item
        else
            match spec.GetReasonForFailure(item) with
            | Some reason -> Error reason
            | None -> Error "仕様を満たしていません"

    // 🤝 複数の仕様をすべて満たすかチェック
    // 【F#初学者向け解説】
    // List.fold は「畳み込み」と呼ばれる操作で、リストの各要素を
    // 順番に処理しながら1つの結果に集約します。
    //
    // この関数の動作:
    // 1. 初期値として Ok item を設定
    // 2. 各仕様を順番に適用
    // 3. 1つでも失敗したら Error を返す（以降の仕様は評価しない）
    // 4. すべて成功したら Ok item を返す
    //
    // 使用例:
    //   let result = satisfiesAll [activeUserSpec; adminRoleSpec] user
    //   match result with
    //   | Ok validUser -> (* すべての仕様を満たすユーザー *)
    //   | Error reason -> (* どこかで失敗した理由 *)
    let satisfiesAll<'T> (specs: ISpecification<'T> list) (item: 'T) : Result<'T, string> =
        specs
        |> List.fold (fun acc spec ->
            // accは累積結果（Result<'T, string>型）
            match acc with
            | Error _ -> acc  // 既に失敗している場合は、そのまま失敗を返す
            | Ok _ -> applySpecification spec item  // まだ成功している場合、次の仕様を適用
        ) (Ok item)  // 初期値: 成功状態