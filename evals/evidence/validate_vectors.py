"""Deterministic fixture validator for Evidence Bundle Schema v0.1.

It validates JSON Schema plus offline semantic rules SEM-001 through SEM-004.
It deliberately does not claim live SCM, workflow, registry, signature, or identity
verification; those require future platform adapters.
"""

from __future__ import annotations

import json
from pathlib import Path

from jsonschema import Draft202012Validator, FormatChecker


ROOT = Path(__file__).resolve().parents[2]
SCHEMA_PATH = ROOT / "docs" / "architecture" / "evidence-bundle.schema.v0.1.json"
VECTORS_PATH = Path(__file__).with_name("vectors.v0.1.json")


def semantic_errors(bundle: dict) -> list[str]:
    """Validate only deterministic, offline semantic rules."""
    errors: list[str] = []
    producer = bundle.get("producer")
    verifier = bundle.get("verifier")
    chain = bundle.get("chain_of_custody") or []

    if producer == verifier:
        errors.append("SEM-001: producer and verifier are not independent")

    if not any(
        event.get("event") == "produced" and event.get("actor") == producer
        for event in chain
    ):
        errors.append("SEM-002: missing producer event")

    if not any(
        event.get("event") == "verified" and event.get("actor") == verifier
        for event in chain
    ):
        errors.append("SEM-002: missing verifier event")

    digest = bundle.get("content_digest")
    if any(event.get("content_digest") != digest for event in chain):
        errors.append("SEM-003: chain digest mismatch")

    return errors


def main() -> None:
    schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
    vectors = json.loads(VECTORS_PATH.read_text(encoding="utf-8"))["vectors"]
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    passed = 0

    for vector in vectors:
        bundle = vector["bundle"]
        structure_errors = sorted(error.message for error in validator.iter_errors(bundle))
        semantic = None if structure_errors else semantic_errors(bundle)
        actual = {
            "structure_valid": not structure_errors,
            "semantic_valid": None if semantic is None else not semantic,
        }
        expected = vector["expected"]
        if actual != expected:
            raise SystemExit(
                f"{vector['case_id']} expected {expected}, got {actual}; "
                f"structure={structure_errors}; semantic={semantic}"
            )
        print(f"{vector['case_id']}: PASS")
        passed += 1

    print(f"EVIDENCE_VECTORS_VALID={passed}")


if __name__ == "__main__":
    main()
