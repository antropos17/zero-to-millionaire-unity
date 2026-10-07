# v0.9 durable migration snapshot and rollback

Close all old game versions and all Editors using the ordinary profile before
the FIRST launch of the updated v0.9. An older already-loaded executable cannot
be retroactively guarded and could still overwrite live save data. No new-version
mutex is claimed to protect old code.

The updated source creates a verified, nonrotating byte-for-byte copy BEFORE
its first successful schema1-5 to schema6 write. The location is
`level1-v1.json.pre-schema6/vN-FINGERPRINT/` next to the active save.
`primary.json` and `backup.json` retain the exact original bytes. `snapshot.txt`
records which originals existed and which source was loaded. Corrupt originals
are preserved when recovery used a valid legacy backup. Original absence is
recorded rather than replaced by a fabricated save.

Different original byte pairs get separate content-addressed version folders.
An existing matching snapshot is reused without rewriting its files. A mismatching
or unreadable snapshot is never overwritten. Required-copy failure prevents the
live save and displays: "Could not protect the old save. No changes saved."
A later live-write failure also leaves both original live files and the completed
snapshot intact. The normal `.bak` can rotate without changing these snapshots.

To roll back, close all games/editors and first preserve the current schema6
profile separately. Read the chosen snapshot's `snapshot.txt`. Copy its original
`primary.json` to `level1-v1.json` and its original `backup.json` to
`level1-v1.json.bak`. If an original was absent, move that corresponding current
live file aside with the preserved schema6 profile. Restore the original pair
together; never leave a schema6 primary in front of a schema5 backup. Then launch
the old version. This returns to the progress before that migration, while the
separate schema6 copy retains later progress for updated v0.9.

Verification used private QA fixtures only. Both .NET and actual Unity JSON
passed155 snapshot checks: schemas1-5, exact bytes including UTF16 BOM, repeated
saves/relaunch, required-copy and live-write failure, migration from corrupt or
absent primary plus valid backup, and separate historical snapshots. Fail-closed cases cover unexpected Decode
failure while Encode remains usable, actual read denial with delete sharing,
external schema6/7 changes after legacy Load, removed originals, future primary
or backup, and confirmed recovery reset/backup rotation. An additional
22 checks used unmodified old Core to read restored runtime snapshots, including
backup-based recovery. Old already-loaded write behavior remains explicitly
documented. All automated QA used private fixtures. A later normal visible game launch was
explicitly requested by the user; it is separate from the private QA results.

Production build source: 457a13f5ecab649aafb371873b1ba9bb301536fe.
The separately built Windows candidate-3 contains this fix. Candidate-1 and
candidate-2 are preserved intermediate builds; do not deliver either as final.
Independent targeted review PASSED this exact source, including the narrowly
scoped already-protected recovery-backup exception. Updated Windows/source
archives contain candidate-3; split-file proofs record exact source and hashes.
No gameplay or economy changes were introduced by these save fixes.
