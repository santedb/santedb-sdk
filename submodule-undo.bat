@ECHO OFF

	SET cwd = %cd%
	FOR /D %%G IN (.\*) DO (
		PUSHD %%G
		IF EXIST ".git" (
			git status
			echo Will revert these changes...
			pause
			git checkout -- *
		)
		POPD
	)
