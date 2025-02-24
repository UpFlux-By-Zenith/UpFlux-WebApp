import { useEffect, useRef } from "react";
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

const hubUrl = "http://localhost:5000/notificationHub"; // Replace with your SignalR hub URL

export const useSubscription = (groupId: string) => {
  const dispatch = useDispatch();
  const connectionStatus = useSelector((root: RootState) => root.connectionStatus.isConnected)
  const isConnectedRef = useRef(false);

  useEffect(() => {
    let connection: signalR.HubConnection | null = null;

    if (!connectionStatus && !isConnectedRef.current) {
      isConnectedRef.current = true; // Mark connection as being established

      CreateSubscription(groupId)
        .then(() => {
          connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();
          dispatch(changeConnectionStatus(true));

          connection.on("ReceiveMessage", (uri: string, message) => {
            if (uri.endsWith("alert")) {
              const parsedData: IAlertMessage = JSON.parse(message);
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
            } else if (uri.endsWith("versions")) {
              const parsedData: IApplications = JSON.parse(message);
              dispatch(updateApps(parsedData))
            } else {
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
            console.log("SignalR disconnected.");
            isConnectedRef.current = false; // Reset on cleanup
          });
        }
      };
    }
  }, [groupId, dispatch, connectionStatus]);
};
