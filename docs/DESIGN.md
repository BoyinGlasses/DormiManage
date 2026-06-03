---
name: Lumina Community
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#594139'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#8d7168'
  outline-variant: '#e1bfb5'
  surface-tint: '#ab3500'
  primary: '#ab3500'
  on-primary: '#ffffff'
  primary-container: '#ff6b35'
  on-primary-container: '#5f1900'
  inverse-primary: '#ffb59d'
  secondary: '#4648d4'
  on-secondary: '#ffffff'
  secondary-container: '#6063ee'
  on-secondary-container: '#fffbff'
  tertiary: '#006c49'
  on-tertiary: '#ffffff'
  tertiary-container: '#00af79'
  on-tertiary-container: '#003a25'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#ffdbd0'
  primary-fixed-dim: '#ffb59d'
  on-primary-fixed: '#390c00'
  on-primary-fixed-variant: '#832600'
  secondary-fixed: '#e1e0ff'
  secondary-fixed-dim: '#c0c1ff'
  on-secondary-fixed: '#07006c'
  on-secondary-fixed-variant: '#2f2ebe'
  tertiary-fixed: '#6ffbbe'
  tertiary-fixed-dim: '#4edea3'
  on-tertiary-fixed: '#002113'
  on-tertiary-fixed-variant: '#005236'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 24px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 26px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 22px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.02em
  headline-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 26px
    fontWeight: '700'
    lineHeight: 32px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 32px
  container-max: 1440px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 24px
---

## Brand & Style

This design system is built for a professional yet vibrant community platform. It prioritizes clarity, intellectual energy, and ease of content consumption. The brand personality is welcoming, expert, and highly organized.

The visual style is **Modern Corporate**, utilizing a high-contrast light mode to ensure long-form readability. It balances crisp, geometric precision with organic warmth through a signature orange primary color and soft, layered depth. The aesthetic is defined by generous whitespace, keeping the focus on the "knowledge chain" of information without visual clutter.

## Colors

The palette is anchored by a high-contrast neutral foundation. The **Primary Brand Color (#FF6B35)** is reserved for high-priority actions and active navigation states.

- **Backgrounds:** Use the primary background for the application canvas and the pure white surface for content containers/cards to create clear separation.
- **Accents:** Use adjusted tag colors (Purple, Green, Blue) with a 10% opacity background and a 100% opacity stroke/text for high readability in light mode.
- **Typography:** Deep charcoal ensures maximum legibility for body text, while slate grey is used for metadata and secondary information to reduce visual noise.

## Typography

This design system uses a dual-font strategy. **Plus Jakarta Sans** provides a friendly, modern personality for headlines and brand-heavy elements. **Inter** is utilized for body text and functional labels due to its exceptional legibility and systematic character.

Maintain a vertical rhythm by strictly adhering to the defined line heights. For long-form community posts, use `body-lg` to prevent reader fatigue. Use `label-sm` in all-caps for utility categories or section headers.

## Layout & Spacing

The design system employs a **12-column fluid grid** for desktop and a **single-column vertical stack** for mobile.

- **Desktop:** 1440px max-width, 32px side margins, and 24px gutters.
- **Tablet:** 8-column grid with 24px margins.
- **Mobile:** 4-column grid with 16px margins.

Spacing follows a strict 4px base unit. Content cards should use `stack-lg` (24px) for internal padding to maintain the "high whitespace" aesthetic. Grouped sidebar items should use `stack-sm` (8px) to indicate relatedness.

## Elevation & Depth

Hierarchy is established through **Ambient Shadows** and tonal shifts rather than heavy borders.

- **Level 0 (Floor):** Primary background (#F8F9FA).
- **Level 1 (Cards):** Pure white surface with a soft, diffused shadow: `0px 4px 20px rgba(0, 0, 0, 0.04)`.
- **Level 2 (Hover/Active):** Increased shadow depth: `0px 8px 30px rgba(0, 0, 0, 0.08)` to indicate interactivity.
- **Level 3 (Modals/Popovers):** High-diffusion shadow: `0px 20px 50px rgba(0, 0, 0, 0.12)`.

Avoid using solid borders on cards; use the shadow and the subtle contrast against the light grey background to define boundaries.

## Shapes

The design system uses a **Rounded** (Level 2) shape language to maintain a modern, approachable feel.

- **Standard Buttons & Inputs:** 0.5rem (8px).
- **Content Cards & Large Containers:** 1rem (16px).
- **Featured Banners:** 1.5rem (24px).
- **Chips & Tags:** Fully pill-shaped (999px) to distinguish them from interactive buttons.

Avatars should always be circular to provide a soft contrast to the structured, rectangular grid.

## Components

### Buttons
- **Primary:** Solid #FF6B35 with white text. 0.5rem radius.
- **Secondary:** Transparent background with #FF6B35 border and text.
- **Ghost:** No border, #64748B text, light grey hover state.

### Input Fields
- White background, 1px border (#E2E8F0), 0.5rem radius. Focus state uses a 2px #FF6B35 border with a soft orange outer glow.

### Cards
- Pure white background, 1rem radius, Level 1 shadow. Use `stack-lg` padding. Headers within cards should use `headline-sm`.

### Chips / Tags
- High-readability light mode style: Color background at 10% opacity with matching 100% opacity text. No border.

### Sidebar Navigation
- Icons should be 24px, using #64748B. Active state uses #FF6B35 for both the icon and a subtle 3px vertical "indicator bar" on the far left or right of the item.

### Lists
- Clean rows with 1px #F1F5F9 bottom borders. Avoid borders on the last item of a list. Use `body-md` for list content.
