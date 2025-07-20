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