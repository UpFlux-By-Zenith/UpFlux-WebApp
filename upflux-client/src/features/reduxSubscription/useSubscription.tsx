import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch, useSelector } from "react-redux";
import { addMetrics, MonitoringData } from "./metricsSlice";
import { CreateSubscription } from "../../api/applicationsRequest";
import { addAlert, IAlertMessage } from "./alertSlice";
import { notifications } from "@mantine/notifications";
import { changeConnectionStatus } from "./connectionSlice";
import { RootState } from "./store";

const hubUrl = "http://localhost:5000/notificationHub"; // Replace with your SignalR hub URL

export const useSubscription = (groupId: string) => {
  const dispatch = useDispatch();
  const connectionStatus = useSelector((root: RootState) => root.connectionStatus)
  useEffect(() => {
    let connection: signalR.HubConnection | null = null;
    if (connectionStatus) {

      // Step 1: Call CreateSubscription to initialize the group ID on the server
      CreateSubscription(groupId)
        .then(() => {
          // Step 2: Build and start the SignalR connection
          connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

          connection.on("ReceiveMessage", (uri: string, message) => {
            if (uri.endsWith("/alert")) {
              const parsedData: IAlertMessage = JSON.parse(message);

              notifications.show({
                title: parsedData.source,
                message: parsedData.message,
                position: "top-right",
              })

              dispatch(addAlert(parsedData))
            } else {

              const parsedData: MonitoringData = JSON.parse(message);
              dispatch(addMetrics(parsedData));
            }
          });

          return connection.start();
        })
        .then(() => {
          console.log("SignalR connected.");

          // Step 3: Invoke CreateGroup on the SignalR hub to join the group
          if (connection) {
            connection.invoke("CreateGroup", groupId).catch((err) => {
              console.error("Error invoking CreateGroup:", err);
            });
            dispatch(changeConnectionStatus(true))
          }
        })
        .catch((err) => {
          console.error("SignalR connection or subscription error:", err);
        });

      // Step 4: Cleanup the SignalR connection
      return () => {
        if (connection) {
          dispatch(changeConnectionStatus(false))
          connection.stop().then(() => console.log("SignalR disconnected."));
        }
      };
    }
  }, [groupId, dispatch]);
};
