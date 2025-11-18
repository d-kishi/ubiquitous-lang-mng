import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright Configuration for Ubiquitous Language Manager E2E Tests
 *
 * ViewportSize: 1920x1080 (Full HD) - NavMenu折りたたみなし確認用
 * Headless: true - DevContainer環境対応
 * BaseURL: https://localhost:5001
 */
export default defineConfig({
  testDir: '.',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'https://localhost:5001',
    trace: 'on-first-retry',
    viewport: { width: 1920, height: 1080 }, // Full HD
    ignoreHTTPSErrors: true, // 自己署名証明書対応
  },

  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1920, height: 1080 }, // Full HD明示
      },
    },
  ],
});
