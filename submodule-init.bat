@ECHO OFF
git submodule init
echo Update Remotes
git submodule update --remote
	FOR /D %%G IN (.\*) DO (
		echo Checkout master %%G
		PUSHD %%G
		IF EXIST ".git" (
			ECHO Checkout master for %%G
			git checkout master
		)
		POPD
	)
