cd ..\..\ClientPublish\Bin\Assets\Plugins\Android\libs\armeabi-v7a\
svn commit -m "auto commit compiled android so" libClientDLL.so

cd ..\arm64-v8a\
svn commit -m "auto commit compiled android 64so" libClientDLL.so
cd ..\..\..\..\..\..\..\