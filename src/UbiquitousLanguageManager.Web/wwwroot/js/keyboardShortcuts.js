/**
 * キーボードショートカット管理システム
 * 
 * Blazor Server初学者向け解説:
 * このJavaScriptモジュールは、Blazorコンポーネントからキーボードイベントを
 * 受け取り、適切なショートカット処理を実行します。
 * C# IJSRuntime経由でこのJavaScript関数群が呼び出されます。
 */

window.keyboardShortcuts = {
    dotNetRef: null,
    isInitialized: false,
    
    /**
     * キーボードショートカットシステムの初期化
     * @param {DotNetObjectReference} dotNetRef - .NETオブジェクト参照
     */
    initialize: function(dotNetRef) {
        if (this.isInitialized) {
            console.warn('KeyboardShortcuts already initialized');
            return;
        }
        
        this.dotNetRef = dotNetRef;
        this.isInitialized = true;
        
        // グローバルキーイベントリスナーを追加
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
        
        console.log('KeyboardShortcuts initialized successfully');
    },
    
    /**
     * キーボードイベントハンドラー
     * @param {KeyboardEvent} event - キーボードイベント
     */
    handleKeyDown: function(event) {
        if (!this.dotNetRef || !this.isInitialized) {
            return;
        }
        
        // 入力フィールドにフォーカスがある場合は一部のショートカットを無効化
        const activeElement = document.activeElement;
        const isInputFocused = activeElement && (
            activeElement.tagName === 'INPUT' ||
            activeElement.tagName === 'TEXTAREA' ||
            activeElement.contentEditable === 'true'
        );
        
        // 入力フィールドでも有効なショートカット
        const allowedInInput = ['Escape', 'F1'];
        
        if (isInputFocused && !allowedInInput.includes(event.key)) {
            // Ctrl+キーの組み合わせは入力フィールド内でも有効
            if (!event.ctrlKey) {
                return;
            }
        }
        
        try {
            // .NETのハンドラーを呼び出し
            this.dotNetRef.invokeMethodAsync('HandleKeyPress', 
                event.key, 
                event.ctrlKey, 
                event.altKey, 
                event.shiftKey
            ).then(() => {
                // ショートカットが処理された場合、デフォルト動作を防ぐ
                // 特定のキーのみデフォルト動作を防ぐ
                if (this.shouldPreventDefault(event)) {
                    event.preventDefault();
                }
            }).catch(error => {
                console.error('Error calling .NET HandleKeyPress:', error);
            });
        } catch (error) {
            console.error('Error in handleKeyDown:', error);
        }
    },
    
    /**
     * デフォルト動作を防ぐべきかどうかを判定
     * @param {KeyboardEvent} event - キーボードイベント
     * @returns {boolean} デフォルト動作を防ぐかどうか
     */
    shouldPreventDefault: function(event) {
        // Ctrl+キーの組み合わせの場合
        if (event.ctrlKey) {
            const preventKeys = ['n', 'f', 'r', 's'];
            return preventKeys.includes(event.key.toLowerCase());
        }
        
        // その他の特定キー
        const preventKeys = ['F1'];
        return preventKeys.includes(event.key);
    },
    
    /**
     * ショートカットシステムの破棄
     */
    dispose: function() {
        if (!this.isInitialized) {
            return;
        }
        
        // イベントリスナーを削除
        document.removeEventListener('keydown', this.handleKeyDown.bind(this));
        
        // 参照をクリア
        this.dotNetRef = null;
        this.isInitialized = false;
        
        console.log('KeyboardShortcuts disposed');
    },
    
    /**
     * デバッグ用: 現在の状態を取得
     * @returns {object} 現在の状態
     */
    getStatus: function() {
        return {
            isInitialized: this.isInitialized,
            hasDotNetRef: this.dotNetRef !== null
        };
    },
    
    /**
     * 手動でショートカットをトリガー（テスト用）
     * @param {string} key - キー
     * @param {boolean} ctrlKey - Ctrlキーが押されているか
     * @param {boolean} altKey - Altキーが押されているか
     * @param {boolean} shiftKey - Shiftキーが押されているか
     */
    triggerShortcut: function(key, ctrlKey = false, altKey = false, shiftKey = false) {
        if (!this.dotNetRef || !this.isInitialized) {
            console.warn('KeyboardShortcuts not initialized');
            return;
        }
        
        try {
            this.dotNetRef.invokeMethodAsync('HandleKeyPress', key, ctrlKey, altKey, shiftKey);
        } catch (error) {
            console.error('Error triggering shortcut:', error);
        }
    }
};

// ページロード時にシステムが利用可能であることをログに記録
console.log('KeyboardShortcuts module loaded and ready');

/**
 * トースト通知システム
 * 
 * Blazor Server初学者向け解説:
 * このオブジェクトは、Blazorコンポーネントから呼び出されるトースト通知機能を提供します。
 * Bootstrap 5のToast APIを使用して、ユーザーフレンドリーな通知を表示します。
 */
window.toastNotifications = {
    
    /**
     * トースト通知コンテナの初期化
     * Bootstrap 5 のToast機能を利用
     */
    initializeToastContainer: function() {
        // トースト通知コンテナがまだ存在しない場合、作成する
        if (!document.getElementById('toast-container')) {
            const toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '1055'; // Bootstrap modalより上
            document.body.appendChild(toastContainer);
        }
    },
    
    /**
     * トースト通知HTMLテンプレートの作成
     * @param {string} id - トーストのユニークID
     * @param {string} type - トーストタイプ (success, error, info, warning)
     * @param {string} title - トーストタイトル
     * @param {string} message - トーストメッセージ
     * @param {boolean} autoHide - 自動非表示フラグ
     * @returns {string} トーストHTML
     */
    createToastHtml: function(id, type, title, message, autoHide = true) {
        const iconMap = {
            success: 'fas fa-check-circle text-success',
            error: 'fas fa-exclamation-circle text-danger',
            warning: 'fas fa-exclamation-triangle text-warning',
            info: 'fas fa-info-circle text-info'
        };
        
        const backgroundMap = {
            success: 'bg-success text-white',
            error: 'bg-danger text-white',
            warning: 'bg-warning text-dark',
            info: 'bg-info text-white'
        };
        
        const icon = iconMap[type] || iconMap.info;
        const background = backgroundMap[type] || backgroundMap.info;
        const delay = type === 'error' ? 8000 : 4000; // エラーは長めに表示
        
        return `
            <div id="${id}" 
                 class="toast" 
                 role="alert" 
                 aria-live="assertive" 
                 aria-atomic="true" 
                 data-bs-autohide="${autoHide}" 
                 data-bs-delay="${delay}">
                <div class="toast-header ${background}">
                    <i class="${icon} me-2"></i>
                    <strong class="me-auto">${title}</strong>
                    <small class="text-muted">${this.getCurrentTime()}</small>
                    <button type="button" 
                            class="btn-close ${type === 'warning' ? 'btn-close-black' : 'btn-close-white'}" 
                            data-bs-dismiss="toast" 
                            aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;
    },
    
    /**
     * 現在時刻を取得（MMdd HH:mm形式）
     * @returns {string} フォーマットされた時刻
     */
    getCurrentTime: function() {
        const now = new Date();
        const month = (now.getMonth() + 1).toString().padStart(2, '0');
        const day = now.getDate().toString().padStart(2, '0');
        const hours = now.getHours().toString().padStart(2, '0');
        const minutes = now.getMinutes().toString().padStart(2, '0');
        return `${month}${day} ${hours}:${minutes}`;
    },
    
    /**
     * 汎用トースト通知の表示
     * @param {string} type - トーストタイプ
     * @param {string} title - タイトル
     * @param {string} message - メッセージ
     * @param {boolean} autoHide - 自動非表示
     */
    showToast: function(type, title, message, autoHide = true) {
        try {
            // コンテナの初期化
            this.initializeToastContainer();
            
            // ユニークIDの生成
            const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
            
            // トーストHTMLの作成
            const toastHtml = this.createToastHtml(toastId, type, title, message, autoHide);
            
            // DOM要素として追加
            const container = document.getElementById('toast-container');
            container.insertAdjacentHTML('beforeend', toastHtml);
            
            // Bootstrap Toastインスタンスの作成と表示
            const toastElement = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastElement);
            
            // トースト表示
            toast.show();
            
            // 非表示後の自動削除
            toastElement.addEventListener('hidden.bs.toast', function() {
                toastElement.remove();
            });
            
        } catch (error) {
            console.error('Error showing toast:', error);
            // フォールバック：標準アラート
            alert(`${title}: ${message}`);
        }
    }
};

/**
 * Blazor Server向け便利関数群
 * IJSRuntime.InvokeVoidAsync で直接呼び出し可能
 */

/**
 * 成功通知の表示
 * @param {string} message - 成功メッセージ
 */
window.showSuccessToast = function(message) {
    window.toastNotifications.showToast('success', '成功', message, true);
};

/**
 * エラー通知の表示
 * @param {string} message - エラーメッセージ
 */
window.showErrorToast = function(message) {
    window.toastNotifications.showToast('error', 'エラー', message, true);
};

/**
 * 警告通知の表示
 * @param {string} message - 警告メッセージ
 */
window.showWarningToast = function(message) {
    window.toastNotifications.showToast('warning', '警告', message, true);
};

/**
 * 情報通知の表示
 * @param {string} message - 情報メッセージ
 */
window.showInfoToast = function(message) {
    window.toastNotifications.showToast('info', '情報', message, true);
};

/**
 * カスタムトースト通知の表示
 * @param {string} type - タイプ (success, error, warning, info)
 * @param {string} title - タイトル
 * @param {string} message - メッセージ
 * @param {boolean} autoHide - 自動非表示
 */
window.showCustomToast = function(type, title, message, autoHide = true) {
    window.toastNotifications.showToast(type, title, message, autoHide);
};

console.log('Toast notification system loaded and ready');