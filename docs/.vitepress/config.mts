import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "GOILauncher.Multiplayer",
  description: "跨平台的Getting Over It联机Mod",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    outline: 'deep',
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
          { text: '开发指南', link: '/client-development/guide' },
          { text: '初始化插件', link: '/client-development/initialize' },
          { text: '连接服务器', link: '/client-development/connect-to-server' },
          { text: '玩家相关功能', link: '/client-development/player' },
          { text: '聊天相关功能', link: '/client-development/chat' },
          { text: '客户端信息', link: '/client-development/client-info' },
          { text: '启动服务器', link: '/client-development/start-server' },
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/Zonakkis/GOILauncher.Multiplayer' }
    ]
  }
})
