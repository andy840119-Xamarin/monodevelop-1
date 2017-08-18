echo "PATH ======= $PATH"
echo "WORKSPACE ======= $WORKSPACE"

echo $BUILD_REPOSITORY
echo $BUILD_REVISION
echo $BUILD_LANE_MAX_REVISION

if [ "$1" = "checkout" ]; then
	cd ..
	git clone git@github.com:xamarin/release-scripts
	cd release-scripts/wrench
	bundle install --path ~/gems
	cd ../..
    rake -f release-scripts/wrench/monodevelop/Rakefile checkout
    rm -rf release-scripts
elif [ "$1" = "verify-codesign" ]; then
	ruby release-scripts/verify-codesign.rb package
else
	cd .. && mv s monodevelop && cd monodevelop
    make $1
    cd .. && mv monodevelop s && cd s
fi;
