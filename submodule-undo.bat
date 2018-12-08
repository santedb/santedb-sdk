@ECHO OFF

	ECHO WILL UPDATE SUBMODULES WITH "%1"
	SET cwd = %cd%
	FOR /D %%G IN (.\*) DO (
		PUSHD %%G
		IF EXIST ".git" (
			git status
			pause
			git checkout -- *
		)
		POPD
	)
