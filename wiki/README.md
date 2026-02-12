# Wiki Content Directory

This directory contains the source markdown files for the GitHub Wiki.

## 📁 Contents

| File | Description |
|------|-------------|
| `Home.md` | Main wiki home page with navigation |
| `Example-Master-Detail.md` | Master-Detail Pattern example walkthrough |
| `Example-Validation.md` | Complex Validation example walkthrough |
| `Example-Async-Operations.md` | Async/Await patterns example walkthrough |
| `deploy-wiki.ps1` | PowerShell deployment script (Windows) |
| `deploy-wiki.sh` | Bash deployment script (Linux/Mac) |
| `DEPLOY.md` | Manual deployment instructions |
| `README.md` | This file |

## 🚀 Quick Deployment

### Windows (PowerShell)

```powershell
cd wiki
.\deploy-wiki.ps1
```

### Linux/Mac (Bash)

```bash
cd wiki
./deploy-wiki.sh
```

### Manual Deployment

See [DEPLOY.md](DEPLOY.md) for step-by-step manual instructions.

## ✏️ Editing Wiki Pages

1. Edit the markdown files in this directory
2. Test locally by viewing in a markdown previewer
3. Run the deployment script to push changes to GitHub
4. Changes appear immediately at https://github.com/pasysxa/winforms-mvp/wiki

## 📝 Adding New Pages

1. Create a new `.md` file in this directory
2. Use `PascalCase-With-Hyphens.md` naming convention
3. Add navigation link to `Home.md`
4. Run deployment script

Example:

```markdown
<!-- New file: Advanced-Topics.md -->
# Advanced Topics

Your content here...
```

Then in `Home.md`:

```markdown
### Advanced Topics
- [Advanced Topics](Advanced-Topics)
```

## 🔗 Wiki Links

GitHub Wiki uses relative links without the `.md` extension:

```markdown
[Link Text](Page-Name)          <!-- Correct -->
[Link Text](Page-Name.md)       <!-- Wrong -->
[Link Text](./Page-Name)        <!-- Wrong -->
```

## 📸 Images

To add images to wiki pages:

1. Upload image through GitHub Wiki web interface
2. Reference in markdown:

```markdown
![Alt Text](image-name.png)
```

## 🧪 Local Preview

To preview markdown locally:

- **VS Code**: Install "Markdown Preview Enhanced" extension
- **Command Line**: Use `grip` (GitHub README Preview)
  ```bash
  pip install grip
  grip Home.md
  # Opens in browser at http://localhost:6419
  ```

## 📚 Markdown Features

GitHub Wiki supports:

- ✅ Standard Markdown
- ✅ GitHub Flavored Markdown (GFM)
- ✅ Syntax highlighting for code blocks
- ✅ Tables
- ✅ Task lists
- ✅ Emoji (:smile: = 😄)
- ❌ HTML (limited support)
- ❌ Custom CSS

## 🔍 Troubleshooting

**Problem**: Deployment script fails with "repository not found"

**Solution**: Enable Wiki in GitHub Settings first:
1. Go to https://github.com/pasysxa/winforms-mvp/settings
2. Check "Wikis" under Features
3. Save and try again

---

**Problem**: Links between pages are broken

**Solution**: Use relative links without `.md`:
- ✅ `[Example](Example-Page)`
- ❌ `[Example](Example-Page.md)`

---

**Problem**: Script requires authentication

**Solution**: Configure Git credentials:
```bash
git config --global credential.helper cache
# Or use SSH keys for GitHub
```

## 📦 Backup

Wiki content is version-controlled. To backup:

```bash
git clone https://github.com/pasysxa/winforms-mvp.wiki.git backup
```

---

For more information, see [DEPLOY.md](DEPLOY.md)
