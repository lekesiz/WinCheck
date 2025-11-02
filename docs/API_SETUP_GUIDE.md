# WinCheck AI Provider Setup Guide

This guide provides detailed instructions for setting up AI provider API keys for WinCheck.

## Table of Contents
- [Overview](#overview)
- [Which AI Provider Should I Choose?](#which-ai-provider-should-i-choose)
- [OpenAI Setup](#openai-setup)
- [Claude (Anthropic) Setup](#claude-anthropic-setup)
- [Gemini (Google) Setup](#gemini-google-setup)
- [Entering API Keys in WinCheck](#entering-api-keys-in-wincheck)
- [Troubleshooting](#troubleshooting)
- [Cost Management](#cost-management)
- [FAQ](#faq)

---

## Overview

WinCheck requires an AI provider to perform system analysis, generate optimization recommendations, and answer questions about your system. You must configure at least ONE AI provider to use WinCheck's core features.

### Supported AI Providers

| Provider | Model | Strengths | Pricing |
|----------|-------|-----------|---------|
| **OpenAI** | GPT-4 | Most accurate analysis, best explanations | $0.03 per 1K tokens (input), $0.06 per 1K tokens (output) |
| **Claude** | Claude 3.5 Sonnet | Strong reasoning, detailed recommendations | $0.003 per 1K tokens (input), $0.015 per 1K tokens (output) |
| **Gemini** | Gemini 1.5 Pro | Fast responses, good value | Free tier available, then $0.00025 per 1K tokens |

**Estimated costs per scan**:
- Quick Scan: $0.01 - $0.03 per scan
- Deep Scan: $0.05 - $0.15 per scan
- Optimize: $0.10 - $0.30 per execution

---

## Which AI Provider Should I Choose?

### Choose **OpenAI** if:
- ✅ You want the most accurate and detailed analysis
- ✅ You need high-quality natural language explanations
- ✅ Budget is not a primary concern ($10-20/month for regular use)
- ✅ You're already familiar with ChatGPT/OpenAI

### Choose **Claude** if:
- ✅ You want detailed, thoughtful recommendations
- ✅ You prefer strong reasoning over pure speed
- ✅ You want a good balance of quality and cost
- ✅ You value privacy (Anthropic's strong privacy policy)

### Choose **Gemini** if:
- ✅ You want to start with a free tier
- ✅ Budget is a primary concern
- ✅ You need faster response times
- ✅ You're okay with slightly less detailed analysis

**Recommendation for most users**: Start with **Gemini** (free tier), then upgrade to **Claude** or **OpenAI** if you need more detailed analysis.

---

## OpenAI Setup

### Step 1: Create OpenAI Account

1. Visit [https://platform.openai.com/signup](https://platform.openai.com/signup)
2. Click **Sign Up**
3. Enter your email address or use Google/Microsoft account
4. Verify your email address
5. Complete phone verification (required for API access)

### Step 2: Add Payment Method

OpenAI requires a payment method for API access:

1. Go to [https://platform.openai.com/account/billing/overview](https://platform.openai.com/account/billing/overview)
2. Click **Add payment details**
3. Enter credit/debit card information
4. Set spending limits (recommended: $10-20/month for WinCheck)

**Setting Spending Limits**:
1. Go to [Billing Settings](https://platform.openai.com/account/billing/limits)
2. Set **Hard limit**: $20 (prevents unexpected charges)
3. Set **Soft limit**: $10 (receive email warning)
4. Click **Save**

### Step 3: Generate API Key

1. Navigate to [API Keys](https://platform.openai.com/api-keys)
2. Click **+ Create new secret key**
3. Name: "WinCheck" (for easy identification)
4. Permissions: Leave as default (Full access)
5. Click **Create secret key**
6. **IMPORTANT**: Copy the key immediately (starts with `sk-`)
   - You cannot view it again after closing the dialog
   - Store it securely (password manager recommended)

### Step 4: Enter Key in WinCheck

1. Open WinCheck application
2. Navigate to **Settings** page
3. Find **OpenAI API Key** field
4. Paste your API key (Ctrl+V)
5. Click **Validate** button
6. Wait for green ✅ checkmark (indicates successful connection)
7. Select **OpenAI** from AI Provider dropdown
8. Click **Save Settings**

### Verification

Test your setup:
1. Go to **Dashboard**
2. Click **Quick Scan**
3. Should complete in 10-30 seconds with health score and recommendations

---

## Claude (Anthropic) Setup

### Step 1: Create Anthropic Account

1. Visit [https://console.anthropic.com/](https://console.anthropic.com/)
2. Click **Sign Up**
3. Enter email address
4. Verify your email
5. Complete account setup

### Step 2: Add Credits

Claude requires prepaid credits:

1. Go to [Billing](https://console.anthropic.com/settings/billing)
2. Click **Add credits**
3. Minimum: $5 (recommended: $10-20 for WinCheck)
4. Enter payment information
5. Complete purchase

**Note**: Credits don't expire, so you can start small.

### Step 3: Generate API Key

1. Navigate to [API Keys](https://console.anthropic.com/settings/keys)
2. Click **+ Create Key**
3. Name: "WinCheck"
4. Click **Create Key**
5. **IMPORTANT**: Copy the key immediately
   - Starts with `sk-ant-`
   - Cannot be viewed again
   - Store securely

### Step 4: Enter Key in WinCheck

1. Open WinCheck application
2. Go to **Settings** page
3. Find **Claude API Key** field
4. Paste your API key
5. Click **Validate** button
6. Wait for green ✅ checkmark
7. Select **Claude** from AI Provider dropdown
8. Click **Save Settings**

### Verification

1. Dashboard → **Quick Scan**
2. Should see Claude-powered analysis

---

## Gemini (Google) Setup

### Step 1: Create Google Account

If you already have a Gmail account, you can use it. Otherwise:

1. Visit [https://accounts.google.com/signup](https://accounts.google.com/signup)
2. Create Google account
3. Verify email

### Step 2: Access Google AI Studio

1. Visit [https://aistudio.google.com/](https://aistudio.google.com/)
2. Sign in with Google account
3. Accept Terms of Service

### Step 3: Generate API Key

1. Click **Get API Key** (top right)
2. Select **Create API Key in new project** (if first time)
   - OR select existing project
3. Click **Create API Key**
4. Copy the API key
   - Store securely

**Free Tier Limits**:
- 60 requests per minute
- 1,500 requests per day
- Sufficient for most WinCheck usage

**Upgrading to Paid** (optional):
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Enable billing for your project
3. Increases rate limits significantly

### Step 4: Enter Key in WinCheck

1. Open WinCheck
2. Settings → **Gemini API Key** field
3. Paste API key
4. Click **Validate**
5. Select **Gemini** as AI Provider
6. Click **Save Settings**

### Verification

1. Dashboard → **Quick Scan**
2. Should complete successfully with Gemini analysis

---

## Entering API Keys in WinCheck

### General Instructions

1. **Open WinCheck**:
   - Double-click `WinCheck.App.exe`
   - Or search for "WinCheck" in Start Menu

2. **Navigate to Settings**:
   - Click Settings icon (gear) in navigation menu
   - Or press `Ctrl+,` (keyboard shortcut)

3. **Enter API Key**:
   - Select appropriate field (OpenAI/Claude/Gemini)
   - Paste API key from clipboard
   - Do NOT include spaces or quotes

4. **Validate Key**:
   - Click **Validate** button next to key field
   - Wait 2-5 seconds for connection test
   - Status indicators:
     - ✅ Green checkmark: Key is valid
     - ❌ Red X: Key is invalid or network error

5. **Select AI Provider**:
   - Use dropdown menu to select active provider
   - Only one provider active at a time
   - Can switch anytime without re-validation

6. **Save Settings**:
   - Click **Save Settings** button at bottom
   - Settings saved to: `%LocalAppData%\WinCheck\settings.json`

### Settings File Location

Your settings are stored locally:
```
C:\Users\<YourUsername>\AppData\Local\WinCheck\settings.json
```

Example settings file:
```json
{
  "SelectedAIProvider": "OpenAI",
  "OpenAIApiKey": "sk-...",
  "ClaudeApiKey": "",
  "GeminiApiKey": "",
  "EnableProcessMonitoring": true,
  "EnableNetworkMonitoring": true,
  "LastModified": "2025-11-02T20:30:00"
}
```

**Security Note**: API keys are stored in plain text. File permissions restrict access to your user account only, but it's recommended to:
- Use API keys with spending limits
- Don't share your WinCheck settings file
- Revoke keys if you suspect compromise

---

## Troubleshooting

### Error: "Invalid API Key"

**Symptom**: Red ❌ appears after clicking Validate

**Solutions**:

1. **Check key format**:
   - OpenAI keys start with `sk-`
   - Claude keys start with `sk-ant-`
   - Gemini keys are alphanumeric

2. **Verify no extra characters**:
   - No spaces before/after key
   - No quotes around key
   - No line breaks

3. **Check key hasn't expired**:
   - OpenAI: Visit [API Keys](https://platform.openai.com/api-keys) → Check status
   - Claude: Visit [API Keys](https://console.anthropic.com/settings/keys)
   - Gemini: Keys don't expire unless manually revoked

4. **Regenerate key**:
   - Delete old key from provider dashboard
   - Generate new key
   - Enter new key in WinCheck

### Error: "Network connection failed"

**Symptom**: Validation times out or shows network error

**Solutions**:

1. **Check internet connection**:
   ```powershell
   ping google.com
   ping api.openai.com
   ```

2. **Check firewall**:
   - Windows Defender Firewall
   - Allow WinCheck.App.exe through firewall
   - Check corporate/enterprise firewall rules

3. **Check proxy settings**:
   - If behind corporate proxy, configure system proxy
   - WinCheck uses system proxy settings

4. **Try different network**:
   - Switch from Wi-Fi to Ethernet or vice versa
   - Try mobile hotspot to isolate network issue

### Error: "Insufficient credits/quota"

**Symptom**: Validation succeeds but scans fail

**Solutions**:

1. **OpenAI**:
   - Check [Usage](https://platform.openai.com/account/usage)
   - Verify billing is active
   - Add payment method if needed
   - Increase spending limit

2. **Claude**:
   - Check [Billing](https://console.anthropic.com/settings/billing)
   - Add more credits if balance is low

3. **Gemini**:
   - Free tier: Check daily quota (1,500 requests/day)
   - Wait 24 hours for quota reset
   - Or upgrade to paid tier

### Error: "Rate limit exceeded"

**Symptom**: Scans fail with rate limit error

**Solutions**:

1. **Wait before retrying**:
   - OpenAI: Wait 60 seconds between requests
   - Claude: Wait 60 seconds
   - Gemini Free: Max 60 requests/minute

2. **Upgrade plan**:
   - OpenAI: Higher tiers have higher limits
   - Gemini: Enable billing for higher limits

3. **Switch provider temporarily**:
   - Use different AI provider while waiting

---

## Cost Management

### Monitoring Usage

**OpenAI**:
1. Visit [Usage Dashboard](https://platform.openai.com/account/usage)
2. View daily/monthly costs
3. Set up email alerts

**Claude**:
1. Visit [Console](https://console.anthropic.com/)
2. Check credit balance
3. View usage history

**Gemini**:
1. Visit [Google Cloud Console](https://console.cloud.google.com/apis/dashboard)
2. Select your project
3. View API usage metrics

### Reducing Costs

1. **Use Quick Scan instead of Deep Scan**:
   - Quick Scan: ~1/5 the cost of Deep Scan
   - Use for daily checks

2. **Limit optimization frequency**:
   - Run Optimize monthly instead of weekly
   - Manual optimization for specific issues

3. **Choose cost-effective provider**:
   - Gemini: Lowest cost (free tier)
   - Claude: Mid-tier pricing
   - OpenAI: Premium pricing

4. **Disable unnecessary monitoring**:
   - Settings → Disable network monitoring if not needed
   - Reduces background API calls

### Estimated Monthly Costs

**Light usage** (1 Deep Scan/week, 3 Quick Scans/week):
- Gemini: **Free** (within free tier)
- Claude: **$2-5/month**
- OpenAI: **$5-10/month**

**Medium usage** (1 Deep Scan/week, 1 Quick Scan/day, 2 Optimizations/month):
- Gemini: **$1-3/month** (may exceed free tier)
- Claude: **$8-12/month**
- OpenAI: **$15-25/month**

**Heavy usage** (2 Deep Scans/week, 2 Quick Scans/day, 4 Optimizations/month):
- Gemini: **$5-8/month**
- Claude: **$15-25/month**
- OpenAI: **$30-50/month**

---

## FAQ

### Q: Can I use multiple AI providers simultaneously?

**A**: No, only one AI provider can be active at a time. However, you can:
- Store API keys for all three providers
- Switch between them instantly in Settings
- No need to re-validate when switching

### Q: Do I need to pay for all three providers?

**A**: No, you only need ONE provider. Choose based on your budget and quality preferences.

### Q: What happens if my API key runs out of credits?

**A**: WinCheck will show an error message. You'll need to:
1. Add more credits/payment to your AI provider account
2. Or switch to a different AI provider
3. Settings and other non-AI features continue working

### Q: Are my API keys secure?

**A**: Keys are stored locally on your PC in:
- Location: `%LocalAppData%\WinCheck\settings.json`
- Permissions: Your user account only
- Not transmitted except to respective AI provider
- Not logged or sent to WinCheck developers

**Best practices**:
- Set spending limits on your API keys
- Don't share your settings.json file
- Revoke and regenerate keys if you suspect compromise

### Q: Can I use WinCheck without an AI provider?

**A**: No, core features (Dashboard analysis, Optimize, Ask AI) require an AI provider. However:
- Process Monitor works without AI
- Disk Cleanup works without AI
- Service Optimizer works without AI
- Startup Manager works without AI
- Registry Cleaner works without AI

Only AI-powered features require an API key.

### Q: Which provider gives the best analysis?

**A**: Based on testing:
1. **OpenAI GPT-4**: Most accurate, best explanations, highest cost
2. **Claude 3.5 Sonnet**: Strong reasoning, detailed recommendations, mid-cost
3. **Gemini 1.5 Pro**: Fast, good quality, lowest cost

For most users, the quality difference is minimal for WinCheck's use case.

### Q: How do I switch between providers?

**A**:
1. Settings → AI Provider dropdown
2. Select different provider
3. Click Save Settings
4. Changes take effect immediately

### Q: Can I get a refund if I don't like an AI provider?

**A**:
- **OpenAI**: No refunds, but can set low spending limits ($5-10)
- **Claude**: Credits don't expire, keep for future use
- **Gemini**: Free tier available, no payment required to test

**Recommendation**: Start with Gemini's free tier to test WinCheck before committing to paid providers.

### Q: What if I exceed my spending limit?

**A**:
- **OpenAI**: API requests will be rejected, WinCheck shows error
- **Claude**: Requests rejected when credits reach zero
- **Gemini Free**: Requests rejected after daily quota (1,500/day)

You'll need to increase limits or wait for quota reset.

### Q: Can I use API keys from other apps?

**A**: Yes! If you already use:
- ChatGPT Plus (OpenAI): Use your existing API key
- Claude.ai: Use your Anthropic API key
- Google AI services: Use your Gemini API key

API keys are account-wide, not app-specific.

### Q: How do I delete my API key from WinCheck?

**A**:
1. Settings → Clear the API key field
2. Click Save Settings
3. Or delete settings file: `%LocalAppData%\WinCheck\settings.json`

**Important**: This only removes the key from WinCheck. To fully revoke:
- Visit your AI provider's dashboard
- Delete/revoke the API key there

---

## Support

If you encounter issues not covered in this guide:

1. Check [README.md](../README.md) Troubleshooting section
2. Open GitHub issue: [WinCheck Issues](https://github.com/yourusername/wincheck/issues)
3. Include:
   - AI provider used
   - Error message (exact text or screenshot)
   - Steps to reproduce

---

**Last Updated**: November 2025
**Version**: 1.0.0
