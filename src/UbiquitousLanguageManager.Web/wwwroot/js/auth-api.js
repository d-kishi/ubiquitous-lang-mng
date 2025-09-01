/**
 * TECH-006 Headers read-onlyエラー解決: 認証API呼び出しヘルパー
 * 
 * このスクリプトは、Blazor ServerコンポーネントからAPI Controllerを
 * 安全に呼び出すためのヘルパー関数を提供します。
 * 
 * 【HTTPコンテキスト分離効果】
 * - 新しいHTTPリクエスト: SignalRとは独立したAPI呼び出し
 * - CSRF保護: Antiforgeryトークンの自動取得・設定
 * - Cookie設定: レスポンスヘッダーでのCookie操作が安全に実行可能
 */

/**
 * CSRFトークンを取得する
 * @returns {Promise<string>} CSRFトークン
 */
async function getCsrfToken() {
    try {
        const response = await fetch('/api/auth/csrf-token', {
            method: 'GET',
            credentials: 'same-origin'
        });
        
        if (!response.ok) {
            throw new Error('CSRFトークンの取得に失敗しました');
        }
        
        const data = await response.json();
        return data.token;
    } catch (error) {
        console.error('CSRF Token取得エラー:', error);
        throw error;
    }
}

/**
 * 認証APIへのPOSTリクエストを実行する
 * @param {string} url APIエンドポイントURL
 * @param {Object} data リクエストデータ
 * @returns {Promise<Object>} APIレスポンス
 */
async function authApiPost(url, data) {
    try {
        // CSRFトークンを取得
        const csrfToken = await getCsrfToken();
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': csrfToken
            },
            body: JSON.stringify(data),
            credentials: 'same-origin'  // Cookie送信を有効化
        });
        
        const result = await response.json();
        
        // 【デバッグ】C#側からのレスポンス内容確認（一時的）
        console.log('Auth API Response:', result);
        console.log('HTTP Status:', response.status);
        console.log('Response OK:', response.ok);
        
        // 【修正】C#側のレスポンスを正確に処理
        // result.successを優先し、明示的なboolean比較を実行
        return {
            success: result.success === true,  // 明示的なboolean比較
            message: result.message || '処理が完了しました。',
            redirectUrl: result.redirectUrl || null,
            status: response.status
        };
    } catch (error) {
        console.error('Auth API エラー:', error);
        
        // 【エラーハンドリング強化】詳細なエラー分析とユーザーフレンドリーなメッセージ
        let errorMessage = 'ネットワークエラーが発生しました。';
        
        if (error.message.includes('fetch')) {
            errorMessage = 'サーバーとの通信に失敗しました。インターネット接続を確認してください。';
        } else if (error.message.includes('NetworkError') || error.message.includes('Failed to fetch')) {
            errorMessage = 'ネットワークエラーです。しばらく待ってからやり直してください。';
        } else if (error.name === 'AbortError') {
            errorMessage = 'リクエストがタイムアウトしました。再度お試しください。';
        }
        
        return {
            success: false,
            message: errorMessage,
            redirectUrl: null,
            status: 0
        };
    }
}

/**
 * ログインAPI呼び出し
 * @param {string} email メールアドレス
 * @param {string} password パスワード
 * @param {boolean} rememberMe ログイン状態保持フラグ
 * @returns {Promise<Object>} ログイン結果
 */
window.authApi = {
    /**
     * ログイン実行
     */
    async login(email, password, rememberMe = false) {
        return await authApiPost('/api/auth/login', {
            email: email,
            password: password,
            rememberMe: rememberMe
        });
    },

    /**
     * パスワード変更実行
     */
    async changePassword(currentPassword, newPassword, confirmPassword) {
        return await authApiPost('/api/auth/change-password', {
            currentPassword: currentPassword,
            newPassword: newPassword,
            confirmPassword: confirmPassword
        });
    },

    /**
     * ログアウト実行
     */
    async logout() {
        return await authApiPost('/api/auth/logout', {});
    },

    /**
     * ページリダイレクト（Blazor Server対応）
     */
    redirectTo(url) {
        if (url) {
            // Blazor Serverのナビゲーションマネージャーが存在する場合はそれを使用
            if (window.DotNet && window.DotNet.invokeMethod) {
                try {
                    window.DotNet.invokeMethod('NavigateToUrl', url);
                    return;
                } catch (e) {
                    console.warn('Blazor navigation failed, using window.location:', e);
                }
            }
            
            // フォールバック: 通常のページ遷移
            window.location.href = url;
        }
    }
};

// デバッグ用ログ出力（開発環境のみ）
if (window.location.hostname === 'localhost') {
    console.log('🔐 TECH-006 認証API JavaScript ヘルパー が読み込まれました');
    console.log('使用可能な関数: authApi.login(), authApi.changePassword(), authApi.logout()');
}