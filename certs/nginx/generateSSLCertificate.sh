#!/bin/bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout HR.LeaveManagement.Clean.key -out HR.LeaveManagement.Clean.crt
# 產生無密碼的 pfx 檔案
# openssl pkcs12 -export -out HR.LeaveManagement.Clean.pfx -inkey HR.LeaveManagement.Clean.key -in HR.LeaveManagement.Clean.crt
# 產生有密碼的 pfx 檔案
openssl pkcs12 -export -out HR.LeaveManagement.Clean.pfx -inkey HR.LeaveManagement.Clean.key -in HR.LeaveManagement.Clean.crt -passout pass:Testing123!