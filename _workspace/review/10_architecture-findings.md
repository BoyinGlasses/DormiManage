# Architecture Findings

## P1 - Visual QA artifact is not bound to the canonical sample dataset

- Files: `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:157`, `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:518`
- Impact: the rendered review artifact uses a stub dataset whose counts/categories/IDs differ from the HTML ref. That means the screenshot evidence mixes UI fidelity with data drift, so a reviewer cannot claim strict pixel parity from the artifact alone.
- Evidence:
  - ref sample shows `12 / 3 / 9` and rows like `#SP-2024-089`, `Kỹ thuật`, `Dịch vụ`
  - current artifact stub renders `12 / 7 / 5` and rows like `#SP-11111111`, `Tài khoản`, `Khác`
- Smallest fix: add a dedicated review fixture mirroring the canonical ref dataset for screenshot-generation tests while keeping behavioral tests on the broader stub.

## P2 - Screenshot gate is proportional, not true 1:1 viewport-normalized

- Files: `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:161`, `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:168`
- Impact: the WPF review artifact is always captured at `1440x860`, while the canonical PNG is `512x377`. This is enough for visual approximation, but not enough for a strict pixel-perfect claim.
- Smallest fix: add a second normalized capture path that renders the route at the ref viewport/aspect and stores a dedicated sign-off artifact.
