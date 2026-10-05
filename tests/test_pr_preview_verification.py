"""Regression tests for the preview CI verification contract; no network calls."""
import io
import os
import subprocess
import json
from pathlib import Path
import tempfile
import unittest
import urllib.error
from unittest.mock import Mock, patch

from tools.pr_preview_verification import (
    check_public_preview, dist_manifest, preview_identity, verify_version,
)

ID = "885ea141-eea2-4a9c-86e7-3d695b880d19"
URL = "https://885ea141-aba-games-unofficial-ports.negcee.workers.dev"
MESSAGE = "PR48 sha=head artifact=123 run=456.1 tree-sha256=digest"
PROVENANCE = {"pull_request": 48, "head_sha": "head", "artifact_id": "123"}
RECEIPT = {"type": "version-upload", "version": 1,
           "worker_name": "aba-games-unofficial-ports",
           "version_id": ID, "preview_url": URL}


class Response(io.BytesIO):
    status = 200


class PreviewVerificationTests(unittest.TestCase):
    def test_provider_identity(self):
        version = {"id": ID, "annotations": {
            "workers/tag": "pr-48", "workers/message": MESSAGE}}
        verify_version(version, ID, MESSAGE)
        for changed in (
            {**version, "id": "wrong"},
            {**version, "annotations": {}},
            {**version, "annotations": {"workers/tag": "pr-48", "workers/message": "stale"}},
            {**version, "annotations": {"workers/tag": "other", "workers/message": MESSAGE}},
        ):
            with self.subTest(changed=changed), self.assertRaises(ValueError):
                verify_version(changed, ID, MESSAGE)

    def test_machine_readable_receipt(self):
        self.assertEqual(preview_identity(json.dumps(RECEIPT)), (ID, URL))
        for changed in (
            {**RECEIPT, "version": 2},
            {**RECEIPT, "worker_name": "another-worker"},
            {**RECEIPT, "version_id": "bad"},
            {**RECEIPT, "preview_url": URL.replace("885ea141", "pr-48")},
            {**RECEIPT, "preview_url": URL + ".attacker.test"},
        ):
            with self.subTest(changed=changed), self.assertRaises(ValueError):
                preview_identity(json.dumps(changed))
        for value in ("", json.dumps(RECEIPT) + "\n" + json.dumps(RECEIPT)):
            with self.assertRaises(ValueError):
                preview_identity(value)

    def test_public_success(self):
        opener = Mock(side_effect=[Response(json.dumps(PROVENANCE).encode()), Response(b"index")])
        self.assertEqual(check_public_preview(URL, PROVENANCE, opener)["status"], "passed")
        self.assertEqual([call.args[0] for call in opener.call_args_list],
                         [URL + "/pr-preview.json", URL + "/"])

    def test_1010_is_blocked_never_passed_or_retried(self):
        for body in (b"error code: 1010", b"error: 1010", b"Error code: 1010\n"):
            opener = Mock(side_effect=urllib.error.HTTPError(
                URL, 403, "Forbidden", {}, io.BytesIO(body)))
            result = check_public_preview(URL, PROVENANCE, opener)
            self.assertEqual(result["status"], "blocked")
            self.assertEqual(result["cloudflare_error"], 1010)
            self.assertEqual(opener.call_count, 1)

    def test_other_http_errors_fail(self):
        for code, body in ((403, b"Forbidden"), (403, b"error code: 10100"),
                           (404, b"error code: 1010"), (500, b"error code: 1010")):
            opener = Mock(side_effect=urllib.error.HTTPError(
                URL, code, "error", {}, io.BytesIO(body)))
            with self.subTest(code=code, body=body), self.assertRaises(urllib.error.HTTPError):
                check_public_preview(URL, PROVENANCE, opener)

    def test_wrong_public_provenance_and_malformed_json_fail(self):
        for body in (b"{}", b"not json"):
            with self.assertRaises(ValueError):
                check_public_preview(URL, PROVENANCE, Mock(return_value=Response(body)))

    def test_bad_index_status_fails(self):
        response = Response(b"bad index")
        response.status = 503
        with self.assertRaises(ValueError):
            check_public_preview(URL, PROVENANCE, Mock(side_effect=[
                Response(json.dumps(PROVENANCE).encode()), response]))

    def test_network_failures_are_not_success(self):
        with self.assertRaises(urllib.error.URLError):
            check_public_preview(URL, PROVENANCE,
                                 Mock(side_effect=urllib.error.URLError("timeout")))

    def test_manifest_detects_changes_and_symlinks(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            (root / "b").write_bytes(b"two")
            (root / "a").write_bytes(b"one")
            before = dist_manifest(root)
            self.assertEqual([x["path"] for x in json.loads(before)], ["a", "b"])
            self.assertEqual(dist_manifest(root), before)
            (root / "a").write_bytes(b"changed")
            self.assertNotEqual(dist_manifest(root), before)
            (root / "link").symlink_to(root / "a")
            with self.assertRaises(ValueError):
                dist_manifest(root)

    def test_workflow_script_end_to_end(self):
        workflow = Path(".github/workflows/release.yml").read_text()
        script = workflow.split("          python3 - <<'PYTHON'\n")[1].split(
            "          PYTHON\n")[0]
        script = "\n".join(line[10:] for line in script.splitlines())
        config = {"name": "aba-games-unofficial-ports", "compatibility_date": "2026-09-22",
                  "workers_dev": True, "preview_urls": True, "assets": {"directory": "./dist"}}
        original_cwd = Path.cwd()
        for scenario in ("passed", "blocked", "generic403", "wrong-version",
                         "changed-production", "changed-assets", "upload-failed"):
            with self.subTest(scenario=scenario), tempfile.TemporaryDirectory() as temporary:
                root = Path(temporary)
                (root / "dist").mkdir()
                (root / "dist/index.html").write_text("test index")
                (root / "wrangler.jsonc").write_text(json.dumps(config))
                environment = {
                    "CLOUDFLARE_API_TOKEN": "fixture-token",
                    "CLOUDFLARE_ACCOUNT_ID": "fixture-account",
                    "PREVIEW_HEAD_SHA": "head", "WEB_ARTIFACT_ID": "123",
                    "GITHUB_REPOSITORY": "neguse/aba-games-unofficial-ports",
                    "GITHUB_RUN_ID": "456", "GITHUB_RUN_ATTEMPT": "1",
                    "GITHUB_STEP_SUMMARY": str(root / "summary"),
                }
                state = {"deployments": 0}
                def upload(arguments, **kwargs):
                    self.assertEqual(arguments[:5],
                                     ["npx", "--yes", "wrangler@4.146.0", "versions", "upload"])
                    state["message"] = arguments[-1]
                    Path(kwargs["env"]["WRANGLER_OUTPUT_FILE_PATH"]).write_text(
                        json.dumps(RECEIPT) + "\n")
                    if scenario == "changed-assets":
                        Path("dist/index.html").write_text("changed")
                    return subprocess.CompletedProcess(arguments, 1 if scenario == "upload-failed" else 0,
                                                       stdout="fixture upload")
                def request(target, **kwargs):
                    if isinstance(target, str):
                        if scenario in ("blocked", "generic403"):
                            body = b"error code: 1010" if scenario == "blocked" else b"Forbidden"
                            raise urllib.error.HTTPError(target, 403, "Forbidden", {}, io.BytesIO(body))
                        data = Path("dist/pr-preview.json").read_bytes() if target.endswith(
                            "/pr-preview.json") else b"index"
                        return Response(data)
                    if target.full_url.endswith("/subdomain"):
                        result = {"previews_enabled": True}
                    elif target.full_url.endswith("/deployments"):
                        state["deployments"] += 1
                        result = {"deployments": []}
                        if scenario == "changed-production" and state["deployments"] == 2:
                            result = {"deployments": [{"id": "unexpected"}]}
                    elif target.full_url.endswith("/versions/" + ID):
                        result = {"id": "wrong" if scenario == "wrong-version" else ID,
                                  "annotations": {"workers/tag": "pr-48",
                                                  "workers/message": state["message"]}}
                    else:
                        self.fail("Unexpected API endpoint: " + target.full_url)
                    return Response(json.dumps({"success": True, "result": result}).encode())
                try:
                    os.chdir(root)
                    with patch.dict(os.environ, environment), \
                         patch("subprocess.check_output", return_value="head\n"), \
                         patch("subprocess.run", side_effect=upload), \
                         patch("urllib.request.urlopen", side_effect=request), \
                         patch("builtins.print"):
                        # The public helper's injectable default is resolved at definition
                        # time; patch the callable explicitly for this integration fixture.
                        import tools.pr_preview_verification as module
                        original = module.check_public_preview
                        with patch.object(module, "check_public_preview",
                                          side_effect=lambda url, p: original(url, p, request)):
                            if scenario in ("passed", "blocked"):
                                exec(compile(script, "<workflow-preview>", "exec"), {})
                                evidence = json.loads(Path(
                                    "preview-evidence/verification.json").read_text())
                                self.assertEqual(evidence["public_probe"]["status"], scenario)
                                self.assertEqual(evidence["provider_provenance"], "passed")
                            else:
                                with self.assertRaises((ValueError, SystemExit,
                                                        urllib.error.HTTPError,
                                                        subprocess.CalledProcessError)):
                                    exec(compile(script, "<workflow-preview>", "exec"), {})
                finally:
                    os.chdir(original_cwd)


if __name__ == "__main__":
    unittest.main()
