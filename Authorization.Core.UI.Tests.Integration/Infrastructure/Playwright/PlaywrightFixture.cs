﻿/* MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Microsoft.Playwright;
using Microsoft.Playwright.TestAdapter;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure.Playwright;

public class PlaywrightFixture : IAsyncLifetime
{
    private static readonly Task<IPlaywright> _playwrightTask = Microsoft.Playwright.Playwright.CreateAsync();

    public IPlaywright Playwright { get; private set; } = null!;

    public string BrowserName { get; private set; } = null!;

    public IBrowserType BrowserType { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        Playwright = await _playwrightTask.ConfigureAwait(false);
        BrowserName = PlaywrightSettingsProvider.BrowserName;
        BrowserType = Playwright[BrowserName];
        Playwright.Selectors.SetTestIdAttribute("data-testid");
    }

    public virtual Task DisposeAsync()
    {
        Playwright?.Dispose();

        return Task.CompletedTask;
    }
}
