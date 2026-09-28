import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  // GitHub Pages 项目站点部署在 /<repo>/ 子路径下，必须与仓库名一致
  base: '/GOILauncher.Multiplayer/',
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
          {
            text: '快速开始', link: '/introduction/getting-started'
          },
          {
            text: '安装与使用',
            items: [
              {
                text: 'Windows', link: '/installation/windows'
              }
            ]
          }
        ]
      },
      {
        text: '客户端插件开发',
        items: [
          { text: '开发指南', link: '/client-development/guide' },
          { text: '初始化插件', link: '/client-development/initialize' },
          { text: '连接操作', link: '/client-development/connect' },
          { text: '房间操作', link: '/client-development/room' },
          { text: '玩家操作', link: '/client-development/player' },
          { text: '聊天操作', link: '/client-development/chat' },
          { text: '客户端信息', link: '/client-development/client-info' },
          // { text: '启动服务器', link: '/client-development/start-server' },
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/Zonakkis/GOILauncher.Multiplayer' }
    ]
  }
})
