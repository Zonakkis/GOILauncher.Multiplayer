import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "GOILauncher.Multiplayer",
  description: "跨平台的Getting Over It联机Mod",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: '快速开始', link: '/introduction/getting-started' },
      { text: '客户端开发指南', link: '/client-development/guide' }
    ],

    sidebar: [
      {
        text: '介绍',
        items: [
          { text: '快速开始', link: '/introduction/getting-started' }
        ]
      },
      {
        text: '客户端开发',
        items: [
          { text: '开发指南', link: '/client-development/guide' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/Zonakkis/GOILauncher.Multiplayer' }
    ]
  }
})
