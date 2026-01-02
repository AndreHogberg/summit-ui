const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  // Enable console logging from browser
  page.on('console', msg => console.log('BROWSER:', msg.text()));
  
  console.log('Navigating to select demo...');
  await page.goto('http://localhost:5245/select', { waitUntil: 'domcontentloaded' });
  await page.waitForLoadState('networkidle');
  // Wait for Blazor SignalR connection to establish
  await page.waitForTimeout(5000);
  
  // Check if trigger exists
  console.log('Checking for select trigger...');
  const triggerCount = await page.locator('.select-trigger').count();
  console.log('Found triggers:', triggerCount);
  
  if (triggerCount === 0) {
    console.log('No triggers found, taking screenshot');
    await page.screenshot({ path: 'debug-select.png' });
    await browser.close();
    return;
  }
  
  let allPassed = true;
  
  // TEST 1: Keyboard selection
  console.log('\n=== TEST 1: Keyboard Selection (Enter) ===');
  const trigger = page.locator('.select-trigger').first();
  await trigger.click();
  await page.waitForTimeout(1000);
  
  const content = page.locator('[data-ark-select-content]').first();
  let contentVisible = await content.isVisible().catch(() => false);
  console.log('Content visible after click:', contentVisible);
  
  if (contentVisible) {
    console.log('Pressing ArrowDown...');
    await page.keyboard.press('ArrowDown');
    await page.waitForTimeout(200);
    
    console.log('Pressing Enter...');
    await page.keyboard.press('Enter');
    await page.waitForTimeout(1000);
    
    const isContentVisibleAfterEnter = await content.isVisible().catch(() => false);
    console.log('Content visible after Enter:', isContentVisibleAfterEnter);
    
    if (isContentVisibleAfterEnter) {
      console.log('FAIL: Dropdown should have closed (keyboard Enter)');
      allPassed = false;
    } else {
      console.log('PASS: Dropdown closed (keyboard Enter)');
    }
  } else {
    console.log('FAIL: Could not open dropdown');
    allPassed = false;
  }
  
  // TEST 2: Mouse click selection
  console.log('\n=== TEST 2: Mouse Click Selection ===');
  await page.waitForTimeout(500); // Extra wait after previous test
  await trigger.click();
  await page.waitForTimeout(1000);
  
  contentVisible = await content.isVisible().catch(() => false);
  console.log('Content visible after click:', contentVisible);
  
  if (contentVisible) {
    // Click on the third item (Orange)
    const orangeItem = page.locator('[data-ark-select-item][data-value="orange"]').first();
    console.log('Clicking on Orange item...');
    await orangeItem.click();
    await page.waitForTimeout(1000);
    
    const isContentVisibleAfterClick = await content.isVisible().catch(() => false);
    console.log('Content visible after click on item:', isContentVisibleAfterClick);
    
    if (isContentVisibleAfterClick) {
      console.log('FAIL: Dropdown should have closed (mouse click)');
      allPassed = false;
    } else {
      console.log('PASS: Dropdown closed (mouse click)');
    }
  } else {
    console.log('FAIL: Could not re-open dropdown');
    allPassed = false;
  }
  
  // TEST 3: Keyboard selection with Space
  console.log('\n=== TEST 3: Keyboard Selection (Space) ===');
  await trigger.click();
  await page.waitForTimeout(1000);
  
  contentVisible = await content.isVisible().catch(() => false);
  console.log('Content visible after click:', contentVisible);
  
  if (contentVisible) {
    console.log('Pressing ArrowDown twice...');
    await page.keyboard.press('ArrowDown');
    await page.waitForTimeout(100);
    await page.keyboard.press('ArrowDown');
    await page.waitForTimeout(200);
    
    console.log('Pressing Space...');
    await page.keyboard.press(' ');
    await page.waitForTimeout(1000);
    
    const isContentVisibleAfterSpace = await content.isVisible().catch(() => false);
    console.log('Content visible after Space:', isContentVisibleAfterSpace);
    
    if (isContentVisibleAfterSpace) {
      console.log('FAIL: Dropdown should have closed (keyboard Space)');
      allPassed = false;
    } else {
      console.log('PASS: Dropdown closed (keyboard Space)');
    }
  } else {
    console.log('FAIL: Could not re-open dropdown');
    allPassed = false;
  }
  
  // TEST 4: Escape key closes dropdown
  console.log('\n=== TEST 4: Escape Key ===');
  await trigger.click();
  await page.waitForTimeout(1000);
  
  contentVisible = await content.isVisible().catch(() => false);
  console.log('Content visible after click:', contentVisible);
  
  if (contentVisible) {
    console.log('Pressing Escape...');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(500);
    
    const isContentVisibleAfterEsc = await content.isVisible().catch(() => false);
    console.log('Content visible after Escape:', isContentVisibleAfterEsc);
    
    if (isContentVisibleAfterEsc) {
      console.log('FAIL: Dropdown should have closed (Escape)');
      allPassed = false;
    } else {
      console.log('PASS: Dropdown closed (Escape)');
    }
  }
  
  // Summary
  console.log('\n=== SUMMARY ===');
  if (allPassed) {
    console.log('ALL TESTS PASSED');
  } else {
    console.log('SOME TESTS FAILED');
  }
  
  await browser.close();
})();
