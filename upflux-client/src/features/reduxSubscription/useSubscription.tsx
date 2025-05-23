import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch, useSelector } from "react-redux";
import { addMetrics, MonitoringData } from "./metricsSlice";
import { CreateSubscription } from "../../api/applicationsRequest";
import { addAlert, IAlertMessage } from "./alertSlice";
import { notifications } from "@mantine/notifications";
import { changeConnectionStatus } from "./connectionSlice";
import { RootState } from "./store";
import classes from "./notification.module.css"
import { IApplications, updateApps } from "./applicationVersions";
import { GROUP_TYPES, IClusterResponse, IMachineStatus } from "./subscriptionConsts";
import { updateMachine, updateMachineStatus, updateMachineVersion, updatePlotValues } from "./machinesSlice";
import { useAppDispatch } from "./hook";
import { IClusterRecommendation, updateClusterRecommendation } from "./clusterRecommendationSlice";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import { IMachine } from "../../api/reponseTypes";
import { HUB_URL } from "../../api/apiConsts";

export const useSubscription = (groupId: string) => {
  if (!groupId) {
    groupId = sessionStorage.getItem("authToken")
  }
  const dispatch = useAppDispatch();
  const connectionStatus = useSelector((root: RootState) => root.connectionStatus.isConnected)
  const isConnectedRef = useRef(false);

  useEffect(() => {
    let connection: signalR.HubConnection | null = null;

    if (!connectionStatus && !isConnectedRef.current) {
      isConnectedRef.current = true; // Mark connection as being established

      console.log("Creating SignalR connection...");
      console.log("Group ID:", groupId);

      CreateSubscription(groupId)
        .then(() => {
          connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .build();
          dispatch(changeConnectionStatus(true));

          connection.invoke("CreateGroup", groupId)
            .then(() => console.log("Group created:", groupId))
            .catch((err) => console.error("Error invoking CreateGroup:", err));


          connection.on("ReceiveMessage", (uri: string, message) => {
            if (uri === GROUP_TYPES.GENERIC_ALERT) {
              const parsedData: IAlertMessage = JSON.parse(message);
              const { deviceId, version } = extractDeviceInfo(parsedData.message, parsedData.source);

              if (deviceId && version) {
                dispatch(updateMachineVersion({
                  machineId: deviceId,
                  machineName: "",
                  dateAddedOn: "",
                  ipAddress: "",
                  appName: "",
                  currentVersion: version,
                  lastUpdatedBy: ""
                }))
              }

              sendNotification(parsedData)
            } else if (uri === GROUP_TYPES.LICENSE_ALERT) {

            } else if (uri === GROUP_TYPES.UPDATE_ALERT) {
              const parsedData = JSON.parse(message)



            } else if (uri === GROUP_TYPES.RECOMMENDATION_ALERT) {
              const parsedData: IClusterRecommendation = JSON.parse(message)
              parsedData.ClusterId = parsedData.ClusterId === "0" ? "Cluster A" : "Cluster B"
              const notification: IAlertMessage = {
                timestamp: Date.now().toString(),
                level: "Information",
                message: `New AI Recommendation Time is available for Cluster id ${parsedData.ClusterId === "0" ? "Cluster A" : "Cluster B"} `,
                source: "AI Scheduler"
              }

              sendNotification(notification)

              dispatch(updateClusterRecommendation(parsedData))

            } else if (uri === GROUP_TYPES.ROLLBACK_ALERT) {

            } else if (uri === GROUP_TYPES.SCHEDULED_UPDATE_ALERT) {
              //'{ "message": "Scheduled update request for MachineId: c3589340-db6b-11ef-8615-2ccf677985c6, to version: CommandMetadata successfully sent." }'
              const parsedData = JSON.parse(message)

              const notification: IAlertMessage = {
                timestamp: Date.now().toString(),
                level: "Information",
                message: parsedData.message,
                source: "AI Scheduler"
              }


            } else if (uri.includes(GROUP_TYPES.STATUS_ALERT)) {
              const statusAlert: IMachineStatus = JSON.parse(message);

              const parsedData: IAlertMessage = {
                timestamp: Date.now().toString(),
                level: statusAlert.IsOnline ? "Information" : "error",
                message: `QC Machine is now ${statusAlert.IsOnline ? "Online" : "Offline"}`,
                source: statusAlert.DeviceUuid
              }
              sendNotification(parsedData)


              dispatch(updateMachineStatus(statusAlert))

            } else if (uri.startsWith(GROUP_TYPES.RECOMMENDATION_PLOT)) {
              const recommendationReponse: IClusterResponse = JSON.parse(message)

              dispatch(updatePlotValues(recommendationReponse))
            }
            else {
              const parsedData: MonitoringData = JSON.parse(message);
              dispatch(addMetrics(parsedData));
            }
          });

          return connection.start();
        })
        .then(() => {
          console.log("SignalR connected.");
          if (connection) {
            connection.invoke("CreateGroup", groupId).catch((err) => {
              console.error("Error invoking CreateGroup:", err);
            });
          }
        })
        .catch((err) => {
          console.error("SignalR connection or subscription error:", err);
          isConnectedRef.current = false; // Reset on error
        });

      return () => {
        if (connection) {
          connection.stop().then(() => {
            isConnectedRef.current = false; // Reset on cleanup
          });
        }
      };
    }
  }, [groupId, connectionStatus]);


  const sendNotification = (parsedData) => {

    const color = parsedData.level === "Information" ? "blue" : "red";

    if (parsedData.source !== "gateway-patrick-1234") {

      notifications.show({
        title: parsedData.source,
        message: parsedData.message,
        position: "top-right",
        autoClose: 10000,
        color,
        classNames: classes,
      });
    }
    dispatch(addAlert(parsedData));
  }

  const extractDeviceInfo = (alertMessage: string, alertSource: string) => {
    // Extract version (Rollback to version X.Y.Z)
    const versionMatch = alertMessage.match(/(?:Update|Rollback) to version (\d+\.\d+\.\d+)/);
    const version = versionMatch ? versionMatch[1] : null;

    // Extract device ID (from source: "Device-UUID")
    const deviceMatch = alertSource.match(/Device-(.+)/);
    const deviceId = deviceMatch ? deviceMatch[1] : null;

    return { deviceId, version };
  };

};
