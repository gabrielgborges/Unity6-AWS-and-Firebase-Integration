# Unity 6 – Firebase & AWS Integration (Cognito, Lambda, DynamoDB)

This repository solves a **critical integration challenge** for Unity 6 projects:  
Unity 6 adopts **.NET Framework 2.5**, while the official AWS SDKs are built for **.NET Framework 2.0**.  
This version mismatch causes **runtime incompatibilities** and prevents direct use of the SDK—especially affecting:

- AWS Cognito authentication  
- Lambda function triggers  
- DynamoDB operations  

---

## Solution

To bridge this gap, it was implemented **custom HTTP requests** using `UnityWebRequest` to interact directly with **AWS APIs**, ensuring compatibility with Unity 6’s runtime and IL2CPP builds.

All integrations are encapsulated using a **modular Service Locator architecture**, abstracting away complexity for developers and enabling **scalable, maintainable, plug-and-play service management**.

---

## Features & Integrations

- **Firebase Analytics** – Simple event tracking and user device type tracking  
- **AWS Cognito** – Secure user sign-up, login, logout and token refresh flow  
- **AWS Lambda** – Custom function triggers with full request/response management  
- **AWS DynamoDB** – Data storage and retrieval via secure signed requests to a Lambda Function 
- **Service Locator Pattern** – Clean API for switching or mocking services  

---

## Demo

A video demo showcasing **Cognito auth**, **Lambda calls**, and **DynamoDB queries** is available upon request.
