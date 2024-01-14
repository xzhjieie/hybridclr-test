
#!/usr/bin/python3
# -*- coding: UTF-8 -*-
import requests
import json
from datetime import datetime, timedelta, timezone
import sys
from math import fabs
from operator import truediv
from optparse import OptionParser
import os, os.path
import shutil
import re
def flushPrint(text):
    print(text)
    sys.stdout.flush()

# 向钉钉群输出信息
def sendGroupMsg(text):
    build_url = os.getenv('BUILD_URL','')
    text = text + build_url
    flushPrint(text)
    token ="fba01473333c13b5fd78e02066743c129125e79f9dcf2388477e81c257cce87e"
    headers = {'Content-Type': 'application/json;charset=utf-8'}  
    api_url = f"https://oapi.dingtalk.com/robot/send?access_token={token}"
    json_text = {
        "msgtype": "text",
        "text": {
            "content": text
        }
    }
    # 发送并打印信息
    requests.post(api_url, json.dumps(json_text), headers=headers)

script_root = os.path.abspath(os.path.dirname(__file__))


def getSVNBranch():
    result = os.popen('svn info')
    svninfo = result.read()
    array = svninfo.split('\n')
    svnpath = ""
    for line in array :
        index = line.find('Relative URL: ^/')
        if index !=-1:
            svnpath = line.replace('Relative URL: ^/','')
            break
    
    t = svnpath.find('/')
    branch = svnpath[0:t]
    flushPrint(branch)
    return branch

def runCMD(CMD,error,exitNum):
    flushPrint(CMD)
    if os.system(CMD) !=0:
        sendGroupMsg(error)
        sys.exit(exitNum)
    
def svnCommit(text):
    svnCommand='svn ci -m "auto commit libClientDLL.so" '+text
    flushPrint(svnCommand)
    return os.system(svnCommand)

def runUpdateSVNCMD(parent_dir,exitNum,branch):
    svnCommand = "svn revert -R "+parent_dir
    runCMD( svnCommand,"svn revert "+parent_dir+"失败,使用的分支是"+branch, exitNum)
    svnCommand = "svn update "+parent_dir
    runCMD( svnCommand,"svn update "+parent_dir+"失败,使用的分支是"+branch, exitNum)


def svnUpdate(branch):
    parent_dir = "../../ClientPublish/Bin/Assets/Plugins/Android"
    runUpdateSVNCMD(parent_dir,10,branch)

    parent_dir = "../AndroidStudioProject"
    runUpdateSVNCMD(parent_dir,11,branch)
    
    parent_dir = "../Client"
    runUpdateSVNCMD(parent_dir,12,branch)
    
    parent_dir = "../Other"
    runUpdateSVNCMD(parent_dir,13,branch)

    parent_dir = "../Share"
    runUpdateSVNCMD(parent_dir,14,branch)

def checkCPPCompileLog(log_file):
    err = ""
    with open(log_file, 'r') as f:
        lines = f.readlines()      #读取全部内容 ，并以列表方式返回
        for line in lines :
            find = False
            nums= re.findall(r': error: .*', line) 
            if(len(nums) >0):
                find = True

            if not find:
                nums= re.findall(r': error C\d+: .*', line)
                if(len(nums) >0):
                    find = True

            if not find:
                if line.find('LINK : fatal error')!=-1:
                    find = True
            
            if not find:
                nums= re.findall(r'error LNK\d+: .*',line)
                if(len(nums) >0):
                    find = True
            
            if not find:
                if line.find(': undefined reference to ')!=-1:
                    find = True           
            
            if find :
                print(line)
                if line.find('\n') !=-1:
                    err = err +line
                else:
                    err = err +line+'\n'
                
    return err

def main() :
    parser = OptionParser()
    parser.add_option("-s", action="store", dest="sendSuc", help="send succ message")

    #args = ["-s","False"]
    (opts, args) = parser.parse_args()
    
    sendSuc = opts.sendSuc and opts.sendSuc == 'True' or False
    
    flushPrint(str(sendSuc))

    branch = getSVNBranch()
    svnUpdate(branch)
    if os.system("gradlew clean") != 0 :
        sendGroupMsg("编译ClientDLL.so gradlew clean失败,使用的分支是"+branch)
        sys.exit(1)
        return 
    log_file = 'complile.log'
    if os.path.exists(log_file):
        os.remove(log_file)
    buildCommand = f"gradlew :libEsShare:assembleRelease :libRaceTool:assembleRelease :libRaceTool:assembleRelease :libsimplenet:assembleRelease :libCommonShare:assembleRelease :libTask:assembleRelease :libPathFinder:assembleRelease :libTable:assembleRelease :libBaseRace:assembleRelease :libRaceLogic:assembleRelease :ClientDll:assembleRelease >{log_file}"
    flushPrint(buildCommand)
    if os.system(buildCommand) != 0 :
        errortext = checkCPPCompileLog(log_file)
        sendGroupMsg("编译ClientDLL.so gradlew assembleRelease 失败,使用的分支是"+branch+'\n'+errortext)
        sys.exit(2)
        return
    
    parent_dir = "../../ClientPublish/Bin/Assets/Plugins/Android/libs/"
    svncommit = parent_dir +"arm64-v8a/libClientDLL.so"+" "+parent_dir+"armeabi-v7a/libClientDLL.so"
    if svnCommit(svncommit)  != 0 :
        sendGroupMsg("提交 ClientDLL.so失败,使用的分支是"+branch)
        sys.exit(3)
        return
    
    if sendSuc:
        sendGroupMsg("提交 ClientDLL.so 成功,使用的分支是"+branch)

    

if __name__ == '__main__':
    main()
