# Constitution Compliance Checklist

All actions, code changes, and project decisions MUST be reviewed against the following checklist before approval or merge. Each item must be explicitly verified and checked off in PRs, reviews, and planning documents.

---

## 1. Test-Driven Development (TDD)
- [ ] Are all new features and bugfixes covered by tests written before implementation?
- [ ] Is the Red-Green-Refactor cycle followed for all changes?
- [ ] Do all tests pass before merging?

## 2. Specification as Authority
- [ ] Does the implementation strictly follow the latest approved specification?
- [ ] Are all requirements and acceptance criteria documented and testable?
- [ ] Are there any undocumented or ad-hoc features? (If yes, must be removed or documented)

## 3. Clean Coding Standards
- [ ] Is code clear, well-named, and consistently formatted?
- [ ] Are functions small, focused, and have minimal side effects?
- [ ] Are code reviews enforcing these standards?

## 4. Explicit State Modeling
- [ ] Is all system state modeled explicitly and transparently?
- [ ] Are state transitions clear and testable?
- [ ] Is there any hidden or implicit state? (If yes, must be refactored)

## 5. Observability
- [ ] Is structured logging implemented for all critical operations?
- [ ] Are error reporting and traceability in place?
- [ ] Can all key events be audited?

## 6. Documentation & Maintainability
- [ ] Is all code, features, and APIs thoroughly documented?
- [ ] Are inline comments present for complex logic?
- [ ] Is documentation updated with every change?
- [ ] Is maintainability prioritized over premature optimization?

## 7. POC Constraints
- [ ] Are performance and security requirements omitted unless explicitly required?
- [ ] Are best practices for maintainability always followed?

---

**Instructions:**
- Attach this checklist to every PR, review, or major planning document.
- Each item must be checked off or justified if not applicable.
- Non-compliance must be documented and remediated before approval.

---

_Last updated: 2025-12-22 (Constitution v1.0.0)_
