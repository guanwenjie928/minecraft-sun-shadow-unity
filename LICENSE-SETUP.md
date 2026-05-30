# Unity License 配置指南

要让 GitHub Actions 自动构建 WebGL，需要配置 Unity 许可证。

## 方案一：手动激活文件（推荐，最稳定）

### 1. 获取 Unity 激活文件
1. 打开浏览器访问 https://license.unity3d.com/manual
2. 登录你的 Unity 账号（没有的话先注册 https://id.unity.com）
3. 选择 **"Unity Personal Edition"**（免费）
4. 按提示生成 `.ulf` 激活文件（不需要真的在本机激活 Unity）
5. 用文本编辑器打开 `.ulf` 文件，复制全部 XML 内容

### 2. 添加到 GitHub Secrets
1. 打开你的仓库 → **Settings** → **Secrets and variables** → **Actions**
2. 点击 **New repository secret**
3. Name 填写: `UNITY_LICENSE`
4. Secret 粘贴: 上一步复制的 XML 内容
5. 点击 **Add secret**

## 方案二：邮箱+密码（更简单但有 2FA 限制）

如果你没开双因素认证(2FA)，可以直接用邮箱密码：

添加两个 Secrets:
- `UNITY_EMAIL`: 你的 Unity 账号邮箱
- `UNITY_PASSWORD`: 你的 Unity 账号密码

---

配置完成后，每次 push 到 main 分支就会自动构建并部署到:
**https://guanwenjie928.github.io/minecraft-sun-shadow-unity/**

可以在 Actions 标签页查看构建进度。
