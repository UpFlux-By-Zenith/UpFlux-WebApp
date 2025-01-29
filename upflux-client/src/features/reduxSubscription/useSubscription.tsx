import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch } from "react-redux";
import { addMessage, MonitoringData } from "./messageSlice";
import { CreateSubscription } from "../../api/applicationsRequest";

const hubUrl = "http://localhost:5000/notificationHub"; // Replace with your SignalR hub URL

export const useSubscription = (groupId: string) => {
  const dispatch = useDispatch();

  useEffect(() => {
    let connection: signalR.HubConnection | null = null;

    // Step 1: Call CreateSubscription to initialize the group ID on the server
    CreateSubscription(groupId)
      .then(() => {
        // Step 2: Build and start the SignalR connection
        connection = new signalR.HubConnectionBuilder()
          .withUrl(hubUrl)
          .withAutomaticReconnect()
          .build();

        connection.on("ReceiveMessage", (uri, message) => {
          const parsedData: MonitoringData = JSON.parse(message);
          dispatch(addMessage(parsedData));
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
        }
      })
      .catch((err) => {
        console.error("SignalR connection or subscription error:", err);
      });

    // Step 4: Cleanup the SignalR connection
    return () => {
      if (connection) {
        connection.stop().then(() => console.log("SignalR disconnected."));
      }
    };
  }, [groupId, dispatch]);
};
