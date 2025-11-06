# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.1.x   | :white_check_mark: |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of WinCheck seriously. If you discover a security vulnerability, please follow these steps:

### 1. Do Not Publicly Disclose

Please **do not** create a public GitHub issue for security vulnerabilities. Public disclosure can put the entire community at risk.

### 2. Report Privately

Send an email to: **security@wincheck.app** (or create a private security advisory on GitHub)

Include the following information:
- Description of the vulnerability
- Steps to reproduce the issue
- Potential impact
- Suggested fix (if any)
- Your contact information

### 3. Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Timeline**: 30-90 days depending on severity

## Security Considerations

### API Key Storage

**Current Implementation (v1.1.0):**
- API keys are encrypted using Windows Data Protection API (DPAPI)
- Encryption scope: Current user only
- Storage location: `%LOCALAPPDATA%\WinCheck\settings.json`

**Best Practices:**
- Use separate API keys for WinCheck (not your main production keys)
- Set spending limits on AI provider accounts
- Regularly rotate API keys
- Never commit API keys to version control

### Administrator Privileges

WinCheck requires administrator privileges for certain operations:

**Requires Admin:**
- Service optimization
- Registry editing
- System file cleanup
- Process termination (system processes)

**Does NOT Require Admin:**
- AI analysis
- Process monitoring (limited visibility)
- Settings management
- Disk cleanup (user folders only)

**Security Measures:**
- UAC prompts for elevated operations
- All system modifications create automatic backups
- Rollback capability for all changes

### Data Privacy

**Data Sent to AI Providers:**
- System metrics (CPU, RAM, disk usage percentages)
- Process names (e.g., "chrome.exe", not file paths)
- Hardware specifications (CPU model, RAM size)
- Service names and startup programs

**Data NOT Sent:**
- File contents or file paths
- Personal documents
- Passwords or credentials
- Browsing history
- Network traffic contents
- Personal identifiers (username, IP address)

**Local Data Storage:**
- All data stored locally in `%LOCALAPPDATA%\WinCheck\`
- File permissions: Current user only
- No telemetry or analytics sent to WinCheck servers
- No data collection without explicit user action

### Network Security

**Outbound Connections:**
WinCheck makes HTTPS connections to:
- `api.openai.com` (if OpenAI selected)
- `api.anthropic.com` (if Claude selected)
- `generativelanguage.googleapis.com` (if Gemini selected)
- `api.github.com` (for update checks)

**No Other Connections:**
- No analytics servers
- No telemetry servers
- No ads or tracking

### Code Security

**Static Analysis:**
- All code reviewed for security issues
- Input validation on all user inputs
- SQL injection protection (no SQL used)
- Path traversal protection
- Command injection protection

**Dependencies:**
- All NuGet packages from official sources
- Regular dependency updates
- No known vulnerabilities in dependencies (as of v1.1.0)

## Known Security Issues

### Resolved Issues

**v1.1.0:**
- ✅ API keys now encrypted with DPAPI (was plaintext in v1.0.x)
- ✅ Silent exception handling improved with logging
- ✅ HttpClient socket exhaustion fixed (static instances)

### Current Limitations

**Settings File Access:**
- If an attacker gains access to your Windows account, they can decrypt API keys (DPAPI limitation)
- Mitigation: Use Windows account security (strong password, 2FA)

**No Code Signing (yet):**
- Executables are not digitally signed
- Planned for v1.2.0 with Microsoft Store release

## Security Checklist for Users

### Before Using WinCheck:

- [ ] Download only from official GitHub releases
- [ ] Verify file integrity (SHA256 checksums)
- [ ] Run with standard user privileges when possible
- [ ] Review requested permissions before granting admin access

### API Key Security:

- [ ] Use dedicated API keys (not your main keys)
- [ ] Set spending limits on AI provider accounts
- [ ] Rotate keys every 90 days
- [ ] Revoke keys if device is compromised

### Regular Maintenance:

- [ ] Keep WinCheck updated to latest version
- [ ] Review backup files periodically
- [ ] Check crash dumps for sensitive data before sharing
- [ ] Monitor AI provider usage for anomalies

## Security Features

### Implemented (v1.1.0):

- ✅ API key encryption (DPAPI)
- ✅ Input validation and sanitization
- ✅ Automatic backups before system changes
- ✅ Crash dump generation (no sensitive data)
- ✅ Error logging with sensitive data filtering
- ✅ Rollback capability for all modifications

### Planned (v1.2.0+):

- 🔜 Code signing (Microsoft Store)
- 🔜 Two-factor authentication for settings
- 🔜 Audit log for all system modifications
- 🔜 Sandboxed process isolation
- 🔜 Hardware-backed key storage (TPM)

## Compliance

WinCheck is designed with privacy in mind:

**GDPR Compliance:**
- No personal data collection without consent
- No data sharing with third parties (except chosen AI provider)
- Right to deletion (delete settings.json)
- Data portability (JSON format)

**Security Standards:**
- Follows OWASP Top 10 guidelines
- Uses industry-standard encryption (DPAPI)
- Principle of least privilege
- Defense in depth

## Contact

For security concerns:
- Email: security@wincheck.app
- GitHub Security Advisories: [Create Advisory](https://github.com/lekesiz/WinCheck/security/advisories/new)

For general questions:
- GitHub Issues: https://github.com/lekesiz/WinCheck/issues
- Documentation: README.md

---

**Last Updated**: November 6, 2025
**Version**: 1.1.0
**Security Review Date**: November 6, 2025
