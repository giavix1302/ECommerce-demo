# Dev Guidelines

> Version 1.0.0 | Cập nhật: 17/11/2025 | Maintain: HO TIEN KY

---

## 1. GIT FLOW

### Branching Strategy

| Nhánh | Mục đích |
|-------|----------|
| `main` | Production, luôn stable |
| `develop` | Phát triển chính |
| `feature/TASK-id-mô-tả` | Tính năng mới |
| `bugfix/BUG-id-mô-tả` | Sửa bug trên develop |
| `hotfix/id-mô-tả` | Sửa bug khẩn cấp production |
| `release/v1.2.0` | Chuẩn bị release |

**Quy trình:** Tạo nhánh từ `develop` → commit → push → tạo PR → merge → xóa nhánh.

---

### Commit Convention

```
<type>(<scope>): <subject>

<body>       ← tùy chọn, giải thích what & why
<footer>     ← tùy chọn, refs task ID / breaking changes
```

**Types:**

| Type | Dùng khi |
|------|----------|
| `feat` | Tính năng mới |
| `fix` | Sửa bug |
| `docs` | Thay đổi documentation |
| `style` | Format code (không ảnh hưởng logic) |
| `refactor` | Refactor code |
| `perf` | Cải thiện performance |
| `test` | Thêm/sửa test |
| `chore` | Build, dependencies |
| `revert` | Revert commit |

**Scopes thường dùng:** `auth`, `api`, `ui`, `db`, `config`, `deps`

**Ví dụ:**
```
feat(auth): thêm đăng nhập bằng Google
fix(cart): sửa lỗi tính tổng khi có mã giảm giá
perf(api): tối ưu query lấy danh sách sản phẩm
```

**Quy tắc:**
- ✅ Viết thường, không dấu chấm cuối, dùng thì hiện tại
- ✅ Subject ≤ 50 ký tự, body mỗi dòng ≤ 72 ký tự
- ❌ Không commit code lỗi hoặc chưa test
- ❌ Mỗi commit chỉ làm 1 việc cụ thể

---

### Pull Request

**Trước khi tạo PR:**
- [ ] Test kỹ trên local
- [ ] Pull code mới nhất từ nhánh base
- [ ] Resolve conflicts
- [ ] Chạy linter & format
- [ ] Viết/update test cases

**Template PR:**
```markdown
## Mô tả
## Loại thay đổi
- [ ] Bug fix / New feature / Breaking change / Docs
## Checklist
- [ ] Đã test, có test cases, update docs, tuân thủ convention
## Screenshots (nếu có UI)
## Cách test
```

**Quy tắc merge:**
- Cần ≥ 1 approval
- Tất cả comments resolved
- CI/CD pass
- Không tự merge PR của mình
- Feature: Squash and merge | Hotfix: Rebase and merge

---

## 2. CODE CONVENTION

### Naming

| Loại | JavaScript/TS | Python |
|------|--------------|--------|
| Biến & hàm | `camelCase` | `snake_case` |
| Class/Interface | `PascalCase` | `PascalCase` |
| Constant toàn cục | `UPPER_SNAKE_CASE` | `UPPER_SNAKE_CASE` |
| File React | `PascalCase.tsx` | — |
| File utility | `camelCase.js` | — |

- Boolean: bắt đầu bằng `is`, `has`, `can`, `should`
- Tên phải rõ nghĩa, tránh viết tắt khó hiểu

### Code Structure

```
src/
├── components/    # UI (common + features)
├── services/      # API, business logic
├── utils/         # Helper functions
├── hooks/         # Custom hooks
├── constants/     # Constants
├── types/         # Type definitions
└── config/        # Config files
```

**Import order:** external libs → internal modules → relative imports

### Best Practices

- **DRY**: Không lặp code — extract hàm dùng chung
- **KISS**: Đơn giản nhất có thể
- **Magic numbers**: Dùng named constants thay số trần
- **Error handling**: Luôn dùng try/catch cho async, throw lỗi có nghĩa
- File ≤ 300 lines, mỗi file/function làm 1 việc

---

## 3. CODE REVIEW

### Reviewer

- Review trong **24h** (urgent: 1h)
- Phân loại comments:
  - 🔴 **Blocking** — phải fix
  - 🟡 **Suggestion** — nên sửa
  - 💡 **Question** — cần clarify
  - 👍 **Praise** — khen tốt

**Feedback tốt:** Giải thích *tại sao* + gợi ý *cách sửa* cụ thể, focus vào code không phê bình cá nhân.

### Điều kiện merge

- [ ] Không có blocking issues
- [ ] Tests pass
- [ ] Không có bugs rõ ràng
- [ ] Edge cases được xử lý
- [ ] Không duplicate code, không over-engineering

---

## 4. TESTING

### Coverage

- Tối thiểu: **80%**
- Critical paths & utility functions: **100%**

### Test Naming

```js
describe('UserService', () => {
  it('should return token when credentials are valid', async () => {
    // Arrange → Act → Assert
  });
  it('should throw error when email is invalid', async () => { });
});
```

### Rules

- Mỗi test phải độc lập, không share state
- Clean up data sau mỗi test
- Dùng data factories (faker), không hardcode values
- Test cả success và failure cases

---

## 5. DOCUMENTATION

### Code Comments

**Cần comment:**
- Logic phức tạp, workarounds, business rules
- TODO/FIXME (kèm ticket ID), regex, constants không rõ

**Không cần comment:**
- Code tự giải thích được

**JSDoc cho hàm:**
```js
/**
 * @param {number} originalPrice
 * @param {number} discountPercent - 0 đến 100
 * @returns {number}
 * @throws {Error} nếu input không hợp lệ
 */
```

### README (bắt buộc mỗi project)

Phải có: mô tả, prerequisites, cài đặt, chạy app, chạy test, env variables, project structure, deployment, contributors.

### API Docs

Dùng **OpenAPI/Swagger**, document đầy đủ: endpoints, request/response format, error codes, examples.

---

## 6. SECURITY

### Nguyên tắc vàng

- ❌ **KHÔNG BAO GIỜ** commit credentials, API keys, passwords vào Git
- ❌ Không hardcode secrets, không log sensitive data
- ✅ Luôn dùng environment variables
- ✅ Add `.env`, `*.pem`, `*.key` vào `.gitignore`

### Bắt buộc

- Hash password bằng **bcrypt** trước khi lưu
- JWT phải có `expiresIn`
- Validate & sanitize **tất cả** input (Joi/Yup/Zod)
- Dùng **parameterized queries** — tránh SQL injection
- Escape HTML — tránh XSS

### Security Checklist (trước deploy)

- [ ] Không hardcoded secrets
- [ ] HTTPS enabled (production)
- [ ] CORS configured
- [ ] Rate limiting implemented
- [ ] Input validation đầy đủ
- [ ] Security headers configured (dùng `helmet`)
- [ ] Dependencies không có known vulnerabilities
- [ ] Error messages không leak thông tin nhạy cảm

---

## 7. QUALITY & PERFORMANCE

### Definition of Done

Task = Done khi:
- [ ] Code complete, tuân thủ convention
- [ ] Unit tests pass, coverage ≥ 80%
- [ ] Code review approved
- [ ] Không có blocking bugs
- [ ] Documentation updated
- [ ] QA/manual testing passed

### Bug Severity

| Level | Ví dụ | Fix trong |
|-------|-------|-----------|
| 🔴 Critical | App crash, mất data, security breach | Ngay lập tức |
| 🟠 High | Feature chính không hoạt động | 24h |
| 🟡 Medium | Feature có issues nhỏ | 1 tuần |
| 🟢 Low | Cosmetic, typo | Sprint tiếp |

### Performance Targets

| Chỉ số | Target |
|--------|--------|
| API response | < 500ms (p95) |
| Page load (FCP) | < 3s |
| DB query đơn giản | < 100ms |
| Bundle size | < 500KB |
| Lighthouse score | > 90 |

### Code Quality Metrics

| Metric | Target |
|--------|--------|
| Code coverage | ≥ 80% |
| Code duplication | < 5% |
| Cyclomatic complexity | < 10/function |
| PR size | < 400 lines |
| Review time | < 24h |

---

## 8. COMMUNICATION

### Daily Standup (15 phút)

1. Hôm qua làm gì?
2. Hôm nay sẽ làm gì?
3. Có blockers không?

→ Không deep-dive kỹ thuật trong standup. Blockers giải quyết ngay sau đó.

### Task States

`Backlog` → `To Do` → `In Progress` → `In Review` → `Done` | `Blocked`

### Response Time

| Mức độ | Kênh |
|--------|------|
| Urgent (< 1h) | Phone / @mention trực tiếp |
| Important (< 4h) | Direct message |
| Normal | Channel message |
| FYI | Thread reply |

---

> Tài liệu này là **living document** — cập nhật thường xuyên theo feedback và best practices mới.
